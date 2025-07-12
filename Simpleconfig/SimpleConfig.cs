using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleConfig
{
    /// <summary>
    /// Proporciona métodos para parsear texto en formato SimpleConfig.
    /// </summary>
    public static class SimpleConfigParser
    {
        // Regex para identificar los componentes de una línea
        private static readonly Regex CommentRegex = new(@"^\s*(#|//).*"); // Comentarios
        private static readonly Regex BlockRegex = new(@"^\s*(?<name>[a-zA-Z0-9_.]+)(\s+(?<id>[a-zA-Z0-9_.]+))?:\s*$"); // Bloques: NAME: o NAME id:
        private static readonly Regex KeyValueRegex = new(@"^\s*(?<key>[a-zA-Z0-9_.]+)\s*=\s*(?<value>.*)\s*$"); // Clave-valor: key = value

        /// <summary>
        /// Parsea una cadena de texto en formato SimpleConfig y la convierte en un diccionario anidado.
        /// </summary>
        /// <param name="content">El contenido del archivo de configuración.</param>
        /// <returns>Un diccionario que representa la configuración.</returns>
        /// <exception cref="FormatException">Lanzada si se encuentra un error de sintaxis.</exception>
        public static Dictionary<string, object> Parse(string content)
        {
            var root = new Dictionary<string, object>();
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

            // Pilas para manejar el anidamiento de bloques
            var scopeStack = new Stack<Dictionary<string, object>>();
            var indentStack = new Stack<int>();

            scopeStack.Push(root);
            indentStack.Push(-1); // Nivel base para la raíz del documento

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // Ignorar comentarios y líneas vacías
                if (string.IsNullOrWhiteSpace(line) || CommentRegex.IsMatch(line))
                {
                    continue;
                }

                var indent = line.TakeWhile(char.IsWhiteSpace).Count();

                // Salir de los bloques si la sangría disminuye
                while (indent <= indentStack.Peek())
                {
                    indentStack.Pop();
                    scopeStack.Pop();
                }

                var currentScope = scopeStack.Peek();
                var trimmedLine = line.Trim();

                // Intentar parsear como un bloque (NAME:)
                var blockMatch = BlockRegex.Match(trimmedLine);
                if (blockMatch.Success)
                {
                    var blockName = blockMatch.Groups["name"].Value;
                    var blockId = blockMatch.Groups["id"].Value;
                    var newBlock = new Dictionary<string, object>();

                    if (!string.IsNullOrEmpty(blockId)) // Bloque con identificador (SERVER frontend:)
                    {
                        if (!currentScope.ContainsKey(blockName))
                        {
                            currentScope[blockName] = new Dictionary<string, object>();
                        }

                        if (currentScope[blockName] is Dictionary<string, object> parentBlock)
                        {
                            parentBlock[blockId] = newBlock;
                        }
                        else
                        {
                            throw new FormatException($"Error en la línea {i + 1}: La clave '{blockName}' ya existe y no es un bloque agrupador.");
                        }
                    }
                    else // Bloque simple (OWNER:)
                    {
                        currentScope[blockName] = newBlock;
                    }

                    scopeStack.Push(newBlock);
                    indentStack.Push(indent);
                    continue;
                }

                // Intentar parsear como clave-valor (key = value)
                var keyValueMatch = KeyValueRegex.Match(trimmedLine);
                if (keyValueMatch.Success)
                {
                    var key = keyValueMatch.Groups["key"].Value;
                    var valueStr = keyValueMatch.Groups["value"].Value;
                    currentScope[key] = ParseValue(valueStr);
                    continue;
                }

                // Si no coincide con ninguna regla, es un error de sintaxis
                throw new FormatException($"Error de sintaxis en la línea {i + 1}: '{line}'");
            }

            return root;
        }

        /// <summary>
        /// Parsea el string de un valor y lo convierte al tipo de dato correspondiente.
        /// </summary>
        private static object ParseValue(string value)
        {
            var trimmedValue = value.Trim();

            // 1. Intentar parsear como lista (si contiene comas)
            if (trimmedValue.Contains(','))
            {
                // Regex para separar por comas, respetando los valores entre comillas
                var listItems = Regex.Split(trimmedValue, @",(?=(?:[^""]*""[^""]*"")*[^""]*$)")
                    .Select(item => ParseValue(item.Trim())) // Parsea cada elemento recursivamente
                    .ToList();
                return listItems;
            }

            // 2. String (entre comillas)
            if (trimmedValue.StartsWith("\"") && trimmedValue.EndsWith("\""))
                return trimmedValue.Substring(1, trimmedValue.Length - 2);

            // 3. Null
            if (trimmedValue == "null")
                return null;

            // 4. Booleano
            if (bool.TryParse(trimmedValue, out bool boolResult))
                return boolResult;

            // 5. Número (entero o decimal)
            if (long.TryParse(trimmedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out long longResult))
                return longResult;

            if (double.TryParse(trimmedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleResult))
                return doubleResult;

            // 6. Fecha (ISO 8601)
            if (DateTimeOffset.TryParse(trimmedValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateResult))
                return dateResult;

            // Si no es ninguno de los anteriores, se devuelve como string (para valores sin comillas que no son otros tipos)
            // Esto es un fallback, según la especificación estricta, los strings siempre llevan comillas.
            // Se podría lanzar un error aquí si se desea ser más estricto.
            return trimmedValue;
        }
    }
}