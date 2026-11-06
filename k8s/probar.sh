#!/usr/bin/env bash
# Prueba en kind: despliega PostgreSQL y la API, aplica las migraciones con el bundle
# y comprueba /health/ready y una llamada real a la API.
# Uso: k8s/probar.sh ./efbundle   (requiere un clúster kind activo y la imagen hito-api:ci cargada)
set -euo pipefail

# Ruta absoluta antes del cd: así funciona igual con ./efbundle que con una ruta completa.
bundle="$(realpath "${1:?Indica la ruta del bundle de migraciones}")"
cd "$(dirname "$0")"

pids=()
trap 'kill "${pids[@]}" 2>/dev/null || true' EXIT

# Abre un port-forward en segundo plano y espera a que el puerto local responda.
reenviar() {
  kubectl port-forward "$1" "$2" >/dev/null &
  pids+=($!)
  for _ in $(seq 30); do
    (exec 3<>"/dev/tcp/127.0.0.1/${2%%:*}") 2>/dev/null && return 0
    sleep 1
  done
  echo "El port-forward a $1 no respondió" >&2
  return 1
}

# Secretos aleatorios por corrida: no se guardan en ningún lado y mueren con el clúster.
clave_db="$(openssl rand -hex 24)"
conexion_interna="Host=postgres;Database=hito;Username=hito;Password=${clave_db}"
kubectl create secret generic hito-db --from-literal=password="$clave_db"
kubectl create secret generic hito-api \
  --from-literal=connection-string="$conexion_interna" \
  --from-literal=jwt-clave="$(openssl rand -hex 32)"

echo "== PostgreSQL"
kubectl apply -f postgres.yaml
kubectl rollout status deployment/postgres --timeout=120s

echo "== Migraciones con el bundle"
reenviar deployment/postgres 15432:5432
# El bundle arranca Program, que exige una clave JWT aunque no emita tokens.
Jwt__Clave="$(openssl rand -hex 32)" \
  ConnectionStrings__Hito="Host=127.0.0.1;Port=15432;Database=hito;Username=hito;Password=${clave_db}" "$bundle"

echo "== API"
kubectl apply -f api.yaml
if ! kubectl rollout status deployment/hito-api --timeout=180s; then
  kubectl describe pods -l app=hito-api
  kubectl logs -l app=hito-api --tail=50
  exit 1
fi

echo "== Llamadas a través del Service"
reenviar service/hito-api 18080:80
api=http://127.0.0.1:18080

listo="$(curl -fsS "$api/health/ready")"
[ "$listo" = "Healthy" ] || { echo "/health/ready respondió: $listo" >&2; exit 1; }
echo "/health/ready: $listo"

# La demo escribe en la base (usuario, clientes, proyectos) y abre sesión: prueba la API de punta a punta.
token="$(curl -fsS -X POST "$api/demo" | jq -r .accessToken)"
cobros="$(curl -fsS -H "Authorization: Bearer $token" "$api/cobros")"
# Se compara como número: el decimal puede llegar como 76000 o 76000.00.
echo "$cobros" | jq -e '.porCobrar == 76000' >/dev/null || { echo "GET /cobros: $cobros, se esperaba porCobrar = 76000" >&2; exit 1; }
echo "POST /demo y GET /cobros: porCobrar = $(echo "$cobros" | jq .porCobrar)"

echo "Prueba en kind: OK"
