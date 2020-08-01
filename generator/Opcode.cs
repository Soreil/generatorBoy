﻿using System.Collections.Generic;

namespace generator
{
    public struct Opcode
    {
        public byte ID;
        public string mnemonic;
        public int bytes;

        public List<int> cycles;

        //There is also a possible increment flag here, we really need a struct instead 
        //of a tuple at this point since it changes too often.
        public List<Operand> operands;
        public bool immediate;
        public List<(string, string)> flags;

        public override string ToString()
        {
            var cycleStrings = new List<string>();
            foreach (var c in cycles)
                cycleStrings.Add(c.ToString());

            var cycleString = string.Join(' ', cycleStrings);

            var operandStrings = new List<string>();
            foreach (var o in operands)
                operandStrings.Add("\t" + o.Name + " " + o.Pointer);

            var operandString = string.Join('\n', operandStrings);

            var flagStrings = new List<string>();
            foreach (var f in flags)
                flagStrings.Add("\t" + f.Item1 + ":" + f.Item2);

            var flagString = string.Join('\n', flagStrings);

            return string.Join('\n', new List<string>
            {
                mnemonic,
                bytes.ToString(),
                cycleString,
                operandString,
                immediate.ToString(),
                flagString
            });
        }

        public string MakeTag()
        {
            var tag = mnemonic;
            foreach (var t in operands)
            {
                if (t.Increment)
                    tag += "I";

                if (t.Decrement)
                    tag += "D";

                tag += "_";
                tag += t.Pointer ? t.Name : "AT_" + t.Name;
            }

            return tag;
        }

        public string MakePrettyTag()
        {
            var tag = mnemonic;
            foreach (var t in operands)
            {
                tag += " ";
                if (!t.Pointer)
                    tag += "(";

                tag += t.Name;
                if (t.Increment)
                    tag += "+";

                if (t.Decrement)
                    tag += "-";

                if (!t.Pointer)
                    tag += ")";
            }

            return tag;
        }

        //This function is called to make the actual calls, therefore we need values instead of types
        public List<string> MakeFunctionCallArguments()
        {
            var functionArguments = new List<string>();
            foreach (var op in operands)
            {
                var arg = "(" + op.MakeOperandArgumentValue();
                arg += ", ";
                arg += op.Pointer ? "true" : "false";
                arg += ")";
                functionArguments.Add(arg);
            }

            return functionArguments;
        }

        //This function is called for the function skeletons, we need types here instead of values
        public List<string> MakeFunctionConstructorArguments()
        {
            var functionArguments = new List<string>();
            foreach (var op in operands)
            {
                var arg = "(" + op.MakeOperandArgumentType();
                arg += ", ";
                arg += "bool";
                arg += ")";
                functionArguments.Add(arg);
            }

            return functionArguments;
        }
        public string MakeFunction()
        {
            var sig = MakeFunctionSignature();
            var body = MakeFunctionBody();
            return string.Join("\n", new string[] { sig, "{", body, "}" });
        }

        public string MakeFunctionSignature() => "public Action " + mnemonic + "(" + MakeFunctionSignatureParamList() + ")";

        private string MakeFunctionBody() => "return () => { };";

        private string MakeFunctionSignatureParamList()
        {
            var arguments = MakeFunctionConstructorArguments();

            var taggedArguments = new List<string>();
            for (var i = 0; i < arguments.Count; i++)
                taggedArguments.Add(arguments[i] + "p" + i.ToString());

            return string.Join(", ", taggedArguments);
        }

    }
}