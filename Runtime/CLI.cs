using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityGadgets.Mqtt {
#if UNITY_STANDALONE
    /// <summary>
    /// Get flags and args from terminal to Unity
    /// </summary>
    public class CLI {
        readonly string[] _args;

        public CLI() {
            _args = Environment.GetCommandLineArgs();
        }

        public bool HasArg(string name) => _args.Contains(name);

        public string GetArg(string name) {

            for (int i = 0; i < _args.Length; i++) {
                if (_args[i] == name && _args.Length > i + 1) {
                    return _args[i + 1];
                }
            }

            return null;
        }

        public string[] GetArgs(string arg, uint num = 1) {
            for (int i = 0; i < _args.Length; i++) {
                if (_args[i] == arg) {
                    if (_args.Length < i) {
                        throw new Exception($"Cannot get value of CLI argument `{arg}`");
                    }

                    string[] res = new string[num];

                    for (int j = 0; j < num; j++) {
                        res[j] = _args[i + j + 1];
                    }

                    return res;
                }
            }

            return null;
        }

        /// <summary>
        /// Read all fields of given objects from CLI arguments
        /// </summary>
        /// <example>
        /// <code>
        /// class Foo : MonoBehaviour {
        ///     public string name;
        ///
        ///     private void OnEnable() {
        ///         new CommandLineReader().LoadData(this, "foo");
        ///         // will read `--foo-name` and set `name` field.
        ///     }
        /// }
        /// </code>
        /// </example>
        public void LoadData(object data, string _prefix = "") {
            Type type = data.GetType();
            var fields = type.GetFields();
            var askedForHelp = HasArg("--help");
            var help = new List<string>();

            foreach (var field in fields) {
                Type fieldType = field.FieldType;
                var prefix = string.IsNullOrEmpty(_prefix) ? "" : _prefix + "-";
                var name = $"--{prefix}{KebabCase(field.Name)}";
                var argType = ArgType.String;
                if (fieldType == typeof(bool)) argType = ArgType.Bool;
                if (fieldType == typeof(int)) argType = ArgType.Int;
                if (fieldType.IsEnum) argType = ArgType.Enum;

                if (askedForHelp) {
                    var input = argType switch {
                        ArgType.Bool => "<true|1|false|0>",
                        ArgType.Int => "<number>",
                        ArgType.Enum => $"<{string.Join("|", fieldType.GetEnumNames())}>",
                        _ => "<value>",
                    };
                    help.Add($"{name} {input}");
                }

                var arg = GetArg(name);
                if (arg == null) continue;

                switch (argType) {
                    case ArgType.Bool:
                        field.SetValue(data, arg == "true" || arg == "1");
                        break;
                    case ArgType.Int:
                        field.SetValue(data, int.Parse(arg));
                        break;
                    case ArgType.Enum:
                        var value = Enum.Parse(fieldType, arg, true);
                        field.SetValue(data, value);
                        break;
                    default:
                        field.SetValue(data, arg);
                        break;
                };

            }

            if (askedForHelp) {
                Debug.Log($"HELP: {_prefix}\n{string.Join("\t\n", help)}");
            }
        }

        // from https://stackoverflow.com/a/37301354/1254484
        string KebabCase(string str) => KebabRegex.Replace(str, "-$1").Trim().ToLower();
        readonly Regex KebabRegex = new("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", RegexOptions.Compiled);

        enum ArgType {
            String,
            Bool,
            Int,
            Enum,
        }
    }
#endif
}
