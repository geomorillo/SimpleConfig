using SimpleConfig;
using System;
using System.Collections.Generic;

// El propósito de este archivo ahora es demostrar el PARSEO de una configuración existente.

// 1. Definir el contenido de la configuración a parsear.
//    Usamos el ejemplo completo del Readme.md para probar todas las características.
var configContent = """
# Configuración de ejemplo

title = "Mi Aplicación"
version = 1.2

OWNER:
  name = "Jhobanny"
  dob = 1979-05-27T07:32:00-08:00

DATABASE:
  enabled = true
  ports = 5432,5433
  temp_targets:
    cpu = 79.5
    case = 72.0

SERVER frontend:
  ip = "10.0.0.1"
  role = "web"

SERVER backend:
  ip = "10.0.0.2"
  role = "api"
""";

try
{
    // 2. Parsear el contenido usando la nueva clase estática.
    var parsedConfig = SimpleConfigParser.Parse(configContent);

    // 3. Demostrar el acceso a los datos parseados.
    Console.WriteLine("--- Configuración Parseada Correctamente ---");

    // Acceso a valores raíz
    Console.WriteLine($"Título: {parsedConfig["title"]}");
    Console.WriteLine($"Versión: {parsedConfig["version"]}");

    // Acceso a un bloque simple (OWNER)
    var ownerBlock = (Dictionary<string, object>)parsedConfig["OWNER"];
    Console.WriteLine($"Nombre del Dueño: {ownerBlock["name"]}");
    Console.WriteLine($"Fecha de Nacimiento: {ownerBlock["dob"]}");

    // Acceso a un sub-bloque (DATABASE -> temp_targets)
    var databaseBlock = (Dictionary<string, object>)parsedConfig["DATABASE"];
    var tempTargetsBlock = (Dictionary<string, object>)databaseBlock["temp_targets"];
    Console.WriteLine($"Target de CPU en DB: {tempTargetsBlock["cpu"]}");

    // Acceso a bloques agrupados (SERVER)
    var serverGroup = (Dictionary<string, object>)parsedConfig["SERVER"];
    var frontendServer = (Dictionary<string, object>)serverGroup["frontend"];
    var backendServer = (Dictionary<string, object>)serverGroup["backend"];
    Console.WriteLine($"IP del Servidor Frontend: {frontendServer["ip"]}");
    Console.WriteLine($"Rol del Servidor Backend: {backendServer["role"]}");

    Console.WriteLine("\n¡El parser funciona como se esperaba!");
}
catch (FormatException ex)
{
    Console.WriteLine($"Error al parsear la configuración: {ex.Message}");
}