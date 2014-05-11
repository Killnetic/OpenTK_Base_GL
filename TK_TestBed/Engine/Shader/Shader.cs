using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using OpenTK.Graphics.OpenGL;

namespace TK_TestBed.Engine.Shader
{
    public abstract class Shader : ICloneable
    {

        public int ProgramID;
        private int vertexShaderID, fragmentShaderID;
        public string VertexShader;
        public string FragmentShader;

        // TODO: make shaders singletons
        //
        private static Dictionary<Type,Shader> compiledShaders = new Dictionary<Type, Shader>();

        // TODO: further genericify to support geometry etc shaders
        //
        public static void Build(Shader shader)
        {
            Type shaderType = shader.GetType();
            Console.WriteLine("shader type: " + shaderType);
            
            shader.ProgramID = GL.CreateProgram();

            LoadShader(shader.VertexShader, ShaderType.VertexShader, shader.ProgramID, out shader.vertexShaderID);
            LoadShader(shader.FragmentShader, ShaderType.FragmentShader, shader.ProgramID, out shader.fragmentShaderID);

            GL.LinkProgram(shader.ProgramID);
            Console.WriteLine("Linked. " + GL.GetProgramInfoLog(shader.ProgramID));

            // programs linked, now assign shader locations
            Dictionary<string, int> sVar = GetShaderBindableVars(shader.VertexShader + "\n" + shader.FragmentShader, shader.ProgramID);

            // Bind known locations to shader class' int members
            //
            
            FieldInfo[] fields = shaderType.GetFields();
            foreach (var field in fields)
            {
                string name = field.Name; // Get string name
                if (field.FieldType == typeof(int))
                {
                    if (sVar.ContainsKey(name))
                    {
                        if (sVar[name] != -1)
                        {
                            field.SetValue(shader, sVar[name]);
                            Console.WriteLine("set field: {0} to location: {1}", name, sVar[name]);
                        }
                        else
                        {
                            var problem = String.Format("!!! unable to set field: {0}, location unknown(-1) !!!", name);
                            Console.WriteLine(problem);
                            Console.ReadKey(true);
                            throw new Exception(problem);
                        }
                    }
                    else
                    {
                        Console.WriteLine("!!! int field found: {0}, unable to find shader location(DNE) !!!", name);
                    }
                }
            }

        }
        private static void LoadShader(string code, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);

            GL.ShaderSource(address, code);

            GL.CompileShader(address);
            GL.AttachShader(program, address);
            var error = GL.GetError();
            if (error != 0)
            {
                string problem = error.ToString();
                Console.WriteLine("\r\n!!! ERROR COMPILING {0} !!!", type);
                Console.WriteLine("\r\n" + problem);
                Console.ReadKey(true);
                throw new Exception(problem);
            }
            int compiled;
            GL.GetShader(address, ShaderParameter.CompileStatus, out compiled);
            if (compiled != 1)
            {
                string problem = GL.GetShaderInfoLog(address);
                Console.WriteLine("\r\n!!! ERROR COMPILING {0} !!!", type);
                Console.WriteLine("\r\n" + problem);
                int i = 0;
                foreach (Match match in Regex.Matches(code, @"^(.*)$", RegexOptions.Multiline))
                {
                    ++i;
                    Console.WriteLine("{0} : {1}", i, match.Groups[1]);
                }
                Console.ReadKey(true);
                throw new Exception(problem);
            }

            Console.WriteLine("Loaded shader\r\n//====\r\n " + code + "\r\n" + GL.GetShaderInfoLog(address));
        }

        static Dictionary<string, int> GetShaderBindableVars(string code, int ShaderProgramID)
        {
            Dictionary<string, int> sVar = new Dictionary<string, int>();

            // [(uniform|in|out|varying)] [name] [gpu location] 
            //
            Dictionary<string, Dictionary<string, int>> vars = new Dictionary<string, Dictionary<string, int>>();

            // Find expected uniforms, in, out, varying vars
            //
            vars.Add("uniform", new Dictionary<string, int>());
            vars.Add("in", new Dictionary<string, int>());
            vars.Add("out", new Dictionary<string, int>());
            vars.Add("varying", new Dictionary<string, int>());
            foreach (Match match in Regex.Matches(code, @"^(uniform|in|out|varying)\s+\w+\s+(\w+)", RegexOptions.Multiline))
            {
                string type = match.Groups[1].Value;
                string name = match.Groups[2].Value;
                vars[type].Add(name, -1);
                // Console.WriteLine("m: " + match);
            }
            // search for conflicts
            //
            foreach (var o in vars["out"].Keys)
            {
                if (vars["in"].ContainsKey(o))
                {
                    vars["in"].Remove(o);
                    Console.WriteLine("!!! found in ({0}) and out ({0}) pair !!!", o);
                }
                else if (vars["varying"].ContainsKey(o))
                {
                    Console.WriteLine("!!! found varying ({0}) and out ({0}) pair !!!", o);
                }
                else if (vars["uniform"].ContainsKey(o))
                {
                    Console.WriteLine("!!! found uniform ({0}) and out ({0}) pair !!!", o);
                }
            }

            // retrieve attrib locations
            //
            foreach (var i in vars["in"].Keys)
            {
                Console.WriteLine("in : {0}", i);
                int loc = GL.GetAttribLocation(ShaderProgramID, i);
                if (loc == -1)
                {
                    var problem = String.Format("!!! unable to get attribute: {0}, location unknown(-1) !!!", i);
                    Console.WriteLine(problem);
                    throw new Exception(problem);
                }

                sVar.Add(i, loc);
            }
            // retrieve uniform locations
            //
            foreach (var u in vars["uniform"].Keys)
            {
                Console.WriteLine("uniform : {0}", u);
                int loc = GL.GetUniformLocation(ShaderProgramID, u);
                if (loc == -1)
                {
                    var problem = String.Format("!!! unable to get uniform: {0}, location unknown(-1) !!!", u);
                    Console.WriteLine(problem);
                    throw new Exception(problem);
                }
                sVar.Add(u, loc);
            }
            return sVar;

        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
