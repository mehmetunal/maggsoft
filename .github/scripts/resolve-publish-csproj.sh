#!/usr/bin/env bash
# Değişen dosya yolundan NuGet'e gönderilecek .csproj yolunu yazar (stdout); eşleşme yoksa çıktı yok.
set -euo pipefail
file="${1:-}"

[[ -z "$file" ]] && exit 0

# Workflow depo kökünde çalışır; yollar repoya görelidir.
rel="$file"

project=""

# Daha dar yollar önce (alt klasörler üst isimle eşleşmesin)
if [[ "$rel" == *"src/Libraries/Aspect/Maggsoft.Aspect.Core"* ]]; then
  project="src/Libraries/Aspect/Maggsoft.Aspect.Core/Maggsoft.Aspect.Core.csproj"
elif [[ "$rel" == *"src/Libraries/Cache/Maggsoft.Cache.MemoryCache"* ]]; then
  project="src/Libraries/Cache/Maggsoft.Cache.MemoryCache/Maggsoft.Cache.MemoryCache.csproj"
elif [[ "$rel" == *"src/Libraries/Cache/Maggsoft.Cache.Redis"* ]]; then
  project="src/Libraries/Cache/Maggsoft.Cache.Redis/Maggsoft.Cache.Redis.csproj"
elif [[ "$rel" == *"src/Libraries/Cache/Maggsoft.Cache"* ]]; then
  project="src/Libraries/Cache/Maggsoft.Cache/Maggsoft.Cache.csproj"

elif [[ "$rel" == *"src/Libraries/Data/Maggsoft.Data.Mongo"* ]]; then
  project="src/Libraries/Data/Maggsoft.Data.Mongo/Maggsoft.Data.Mongo.csproj"
elif [[ "$rel" == *"src/Libraries/Data/Maggsoft.Data.Mssql"* ]]; then
  project="src/Libraries/Data/Maggsoft.Data.Mssql/Maggsoft.Data.Mssql.csproj"
elif [[ "$rel" == *"src/Libraries/Data/Maggsoft.Data.Mysql"* ]]; then
  project="src/Libraries/Data/Maggsoft.Data.Mysql/Maggsoft.Data.Mysql.csproj"
elif [[ "$rel" == *"src/Libraries/Data/Maggsoft.Data.Npgsql"* ]]; then
  project="src/Libraries/Data/Maggsoft.Data.Npgsql/Maggsoft.Data.Npgsql.csproj"
elif [[ "$rel" == *"src/Libraries/Data/Maggsoft.Data.Sqlite"* ]]; then
  project="src/Libraries/Data/Maggsoft.Data.Sqlite/Maggsoft.Data.Sqlite.csproj"
elif [[ "$rel" == *"src/Libraries/Data/Maggsoft.Data"* ]]; then
  project="src/Libraries/Data/Maggsoft.Data/Maggsoft.Data.csproj"

elif [[ "$rel" == *"src/Libraries/Endpoints/Maggsoft.Endpoints"* ]]; then
  project="src/Libraries/Endpoints/Maggsoft.Endpoints/Maggsoft.Endpoints.csproj"

elif [[ "$rel" == *"src/Libraries/EventBus/Maggsoft.EventBus.AzureServiceBus"* ]]; then
  project="src/Libraries/EventBus/Maggsoft.EventBus.AzureServiceBus/Maggsoft.EventBus.AzureServiceBus.csproj"
elif [[ "$rel" == *"src/Libraries/EventBus/Maggsoft.EventBus.IoC"* ]]; then
  project="src/Libraries/EventBus/Maggsoft.EventBus.IoC/Maggsoft.EventBus.IoC.csproj"
elif [[ "$rel" == *"src/Libraries/EventBus/Maggsoft.EventBus.RabbitMQ"* ]]; then
  project="src/Libraries/EventBus/Maggsoft.EventBus.RabbitMQ/Maggsoft.EventBus.RabbitMQ.csproj"
elif [[ "$rel" == *"src/Libraries/EventBus/Maggsoft.EventBus"* ]]; then
  project="src/Libraries/EventBus/Maggsoft.EventBus/Maggsoft.EventBus.csproj"

elif [[ "$rel" == *"src/Libraries/Logging/Maggsoft.Logging"* ]]; then
  project="src/Libraries/Logging/Maggsoft.Logging/Maggsoft.Logging.csproj"

elif [[ "$rel" == *"src/Libraries/Ocelot/Maggsoft.Ocelot.Core"* ]]; then
  project="src/Libraries/Ocelot/Maggsoft.Ocelot.Core/Maggsoft.Ocelot.Core.csproj"

elif [[ "$rel" == *"src/Libraries/Services/Maggsoft.Mongo.Services"* ]]; then
  project="src/Libraries/Services/Maggsoft.Mongo.Services/Maggsoft.Mongo.Services.csproj"
elif [[ "$rel" == *"src/Libraries/Services/Maggsoft.Mssql.Services"* ]]; then
  project="src/Libraries/Services/Maggsoft.Mssql.Services/Maggsoft.Mssql.Services.csproj"
elif [[ "$rel" == *"src/Libraries/Services/Maggsoft.Mysql.Services"* ]]; then
  project="src/Libraries/Services/Maggsoft.Mysql.Services/Maggsoft.Mysql.Services.csproj"
elif [[ "$rel" == *"src/Libraries/Services/Maggsoft.Npgsql.Services"* ]]; then
  project="src/Libraries/Services/Maggsoft.Npgsql.Services/Maggsoft.Npgsql.Services.csproj"
elif [[ "$rel" == *"src/Libraries/Services/Maggsoft.Sqlite.Services"* ]]; then
  project="src/Libraries/Services/Maggsoft.Sqlite.Services/Maggsoft.Sqlite.Services.csproj"
elif [[ "$rel" == *"src/Libraries/Services/Maggsoft.Services"* ]]; then
  project="src/Libraries/Services/Maggsoft.Services/Maggsoft.Services.csproj"

elif [[ "$rel" == *"src/Libraries/Maggsoft.Core"* ]]; then
  project="src/Libraries/Maggsoft.Core/Maggsoft.Core.csproj"
elif [[ "$rel" == *"src/Libraries/Maggsoft.Mongo"* ]]; then
  project="src/Libraries/Maggsoft.Mongo/Maggsoft.Mongo.csproj"
elif [[ "$rel" == *"src/Libraries/Maggsoft.Mssql"* ]]; then
  project="src/Libraries/Maggsoft.Mssql/Maggsoft.Mssql.csproj"
elif [[ "$rel" == *"src/Libraries/Maggsoft.Mysql"* ]]; then
  project="src/Libraries/Maggsoft.Mysql/Maggsoft.Mysql.csproj"
elif [[ "$rel" == *"src/Libraries/Maggsoft.Npgsql"* ]]; then
  project="src/Libraries/Maggsoft.Npgsql/Maggsoft.Npgsql.csproj"
elif [[ "$rel" == *"src/Libraries/Maggsoft.Sqlite"* ]]; then
  project="src/Libraries/Maggsoft.Sqlite/Maggsoft.Sqlite.csproj"

elif [[ "$rel" == *"src/Presentation/Maggsoft.Framework"* ]]; then
  project="src/Presentation/Maggsoft.Framework/Maggsoft.Framework.csproj"

elif [[ "$rel" == *"src/Tools/Test"* ]]; then
  project="src/Tools/Test/Test.csproj"
fi

if [[ -n "$project" ]] && [[ -f "$project" ]]; then
  echo "$project"
fi
