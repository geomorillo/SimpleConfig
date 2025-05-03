using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SimpleConfig
{
    public class SimpleConfig
    {
        public class ConfigBuilder
        {
            private readonly Config _config = new Config();

            public ConfigBuilder AddAssignment(string key, object value)
            {
                if (!IsValidType(value))
                    throw new ArgumentException("Tipo de dato no soportado: " + value.GetType().Name);

                _config.Assignments[key] = FormatValue(value);
                return this;
            }

            public ConfigBuilder AddBlock(string name, Action<BlockBuilder> buildAction)
            {
                var blockBuilder = new BlockBuilder();
                buildAction(blockBuilder);
                _config.Blocks.Add(new ConfigBlock
                {
                    Name = name,
                    Keys = blockBuilder.GetValues()
                });
                return this;
            }

            public string Build()
            {
                var sb = new StringBuilder();
                SerializeBlock(_config, sb, 0);
                return sb.ToString();
            }

            private void SerializeBlock(Config config, StringBuilder sb, int indentLevel)
            {
                foreach (var assignment in config.Assignments)
                {
                    sb.AppendLine($"{new string(' ', indentLevel * 2)}SET {assignment.Key} = {assignment.Value}");
                }

                foreach (var block in config.Blocks)
                {
                    sb.AppendLine($"{new string(' ', indentLevel * 2)}{block.Name}:");
                    SerializeKeyValuePairs(block.Keys, sb, indentLevel + 1);
                }
            }

            private void SerializeKeyValuePairs(Dictionary<string, object> values, StringBuilder sb, int indentLevel)
            {
                foreach (var kvp in values)
                {
                    if (kvp.Value is Dictionary<string, object> nestedBlock)
                    {
                        sb.AppendLine($"{new string(' ', indentLevel * 2)}{kvp.Key}:");
                        SerializeKeyValuePairs(nestedBlock, sb, indentLevel + 1);
                    }
                    else
                    {
                        sb.AppendLine($"{new string(' ', indentLevel * 2)}{kvp.Key} = {kvp.Value}");
                    }
                }
            }

            public string Serialize()
            {
                var sb = new StringBuilder();

                // Generar asignaciones
                foreach (var assignment in _config.Assignments)
                {
                    sb.AppendLine($"SET {assignment.Key} = {assignment.Value}");
                }

                // Generar bloques
                foreach (var block in _config.Blocks)
                {
                    sb.AppendLine($"{block.Name}:");
                    foreach (var key in block.Keys)
                    {
                        sb.AppendLine($"  {key.Key} = {key.Value}");
                    }
                }

                return sb.ToString();
            }

            public static bool IsValidType(object value)
            {
                return value is string || value is bool || value is int || value is double || value is Action<BlockBuilder>;
            }

            public static string FormatValue(object value)
            {
                if (value is string str)
                    return $"\"{str}\"";

                if (value is bool b)
                    return b ? "true" : "false";

                return value.ToString().ToLower();
            }
        }

        public class BlockBuilder
        {
            private readonly Dictionary<string, object> _values = new();

            public BlockBuilder Add(string key, object value)
            {
                if (value is Dictionary<string, object> dict)
                {
                    _values[key] = dict;
                }
                else
                {
                    _values[key] = FormatValue(value);
                }
                return this;
            }

            public BlockBuilder Add(string key, Func<BlockBuilder, object> nestedBlock)
            {
                var builder = new BlockBuilder();
                nestedBlock(builder);
                _values[key] = builder.GetValues();
                return this;
            }

            public Dictionary<string, object> GetValues() => _values;
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                string s => $"\"{s}\"",
                bool b => b ? "true" : "false",
                _ => value.ToString().ToLower()
            };
        }
    }

    public class Config
    {
        public static Config Deserialize(string content)
        {
            return Deserializer.Parse(content);
        }

        public Dictionary<string, object> Assignments { get; } = new Dictionary<string, object>();
        public List<ConfigBlock> Blocks { get; } = new List<ConfigBlock>();
    }

    public class ConfigBlock
    {
        public Dictionary<string, object> Keys { get; set; } = new Dictionary<string, object>();
        public string Name { get; set; }
    }

    public class Deserializer
    {
        public static Config Parse(string content)
        {
            var config = new Config();
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            ConfigBlock currentBlock = null;
            int currentIndent = 0;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                int indent = line.TakeWhile(c => c == ' ').Count() / 2;

                if (trimmedLine.StartsWith("SET "))
                {
                    var parts = trimmedLine.Substring(4).Split('=');
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = ParseValue(parts[1].Trim());
                        config.Assignments[key] = value;
                    }
                }
                else if (trimmedLine.EndsWith(":"))
                {
                    currentBlock = new ConfigBlock { Name = trimmedLine.TrimEnd(':') };
                    config.Blocks.Add(currentBlock);
                    currentIndent = indent;
                }
                else if (indent > currentIndent && currentBlock != null)
                {
                    var keyValue = trimmedLine.Split('=');
                    if (keyValue.Length == 2)
                    {
                        currentBlock.Keys[keyValue[0].Trim()] = ParseValue(keyValue[1].Trim());
                    }
                }
            }
            return config;
        }

        private static object ParseValue(string value)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return value.Substring(1, value.Length - 2);

            if (bool.TryParse(value, out bool boolResult))
                return boolResult;

            if (int.TryParse(value, out int intResult))
                return intResult;

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleResult))
                return doubleResult;

            return value;
        }
    }
}