﻿using System;
using OpenTK.Graphics.OpenGL4;


namespace engenious.Graphics
{
    public enum ShaderType
    {
        FragmentShader = 35632,
        VertexShader,
        GeometryShader = 36313,
        TessEvaluationShader = 36487,
        TessControlShader,
        ComputeShader = 37305
    }

    internal class Shader :IDisposable
    {
        internal int shader;

        public Shader(ShaderType type, string source)
        {
            ThreadingHelper.BlockOnUIThread(()=>{
            shader = GL.CreateShader((OpenTK.Graphics.OpenGL4.ShaderType)type);
            GL.ShaderSource(shader, source);
            });
        }

        internal void Compile()
        {
            ThreadingHelper.BlockOnUIThread(()=>{
            GL.CompileShader(shader);

            int compiled;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out compiled);
            if (compiled != 1)
            {
                string error = GL.GetShaderInfoLog(shader);
                throw new Exception(error);
            }
            });
        }

        public void Dispose()
        {
            GL.DeleteProgram(shader);
        }

    }
}

