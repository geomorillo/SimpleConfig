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

            private readonly List<ConfigBlock> _blocks = new();


            public ConfigBuilder AddBlock(string name, Action<BlockBuilder> buildAction)
            {
                var blockBuilder = new BlockBuilder();
                buildAction(blockBuilder);
                _blocks.Add(new ConfigBlock
                {
                    Name = name,
                    Keys = blockBuilder.GetValues()
                });
                return this;
            }

            public string Build()
            {
                var sb = new StringBuilder();
                SerializeBlock(sb, 0);
                return sb.ToString();
            }

            private void SerializeBlock(StringBuilder sb, int indentLevel)
            {

                foreach (var block in _blocks)
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
            //si el value es una lista por ejemplo new[] { 5432, 5433 } debe ser convertido a "5432, 5433"
            //determinar el tipo de objeto
            var type = value.GetType().Name;
            if (value is Int32[] enumerable)
            {
                return string.Join(", ", enumerable);
            }
            //soportar las listas mistas new object[] {"localhost ok", 5432, "433", 24.5 }
            if (value is object[] array)
            {
                var returnValue = "";
                for (int i = 0; i < array.Length; i++)
                {
                    var item = array[i];
                    if (item is string)
                    {
                        returnValue += $"\"{item}\"";
                    }
                    else
                    {
                        returnValue += $"{item}";
                    }
                    if (i < array.Length - 1) // si no es el último elemento, agrega una coma
                    {
                        returnValue += ", ";
                    }
                }
                return returnValue;
            }

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
                // Ignorar líneas en blanco o comentarios según la especificación
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#") || trimmedLine.StartsWith("//"))
                    continue;

                int indent = line.TakeWhile(c => c == ' ').Count() / 2;

                if (trimmedLine.EndsWith(":"))
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