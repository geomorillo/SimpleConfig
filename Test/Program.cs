﻿using SimpleConfig;
using static SimpleConfig.SimpleConfig;

// 1. Construcción de configuración con bloques anidados
var config = new ConfigBuilder()
    .AddAssignment("APP_NAME", "MiAplicación")
    .AddAssignment("DEBUG_MODE", true)
    .AddBlock("DATABASE", db => db
        .Add("host", "localhost")
        .Add("port", 5432)
        .Add("credentials", cred => cred
            .Add("user", "admin")
            .Add("password", "secret")))
    .AddBlock("LOGGING", log => log
        .Add("level", "verbose")
        .Add("retention_days", 7.5))
    .Build();

// 2. Serialización a archivo
File.WriteAllText("config.scfg", config);

// 3. Deserialización desde archivo
var loadedConfig = Deserializer.Parse(File.ReadAllText("config.scfg"));

// Uso de la configuración cargada
Console.WriteLine($"Modo depuración: {loadedConfig.Assignments["DEBUG_MODE"]}");
Console.WriteLine($"Puerto DB: {loadedConfig.Blocks[0].Keys["port"]}");