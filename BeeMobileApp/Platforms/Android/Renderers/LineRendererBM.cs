using Android.Opengl;
using Java.Nio;

namespace BeeMobileApp.Platforms.Android.Renderers
{
    public class LineRendererBM : IDisposable
    {
        private int mProgram;
        bool isDisposed;

        private readonly string vertexShaderCode =
            // This matrix member variable provides a hook to manipulate
            // the coordinates of the objects that use this vertex shader
            "uniform mat4 uMVPMatrix;" +
                    "attribute vec4 vPosition;" +
                    "void main() {" +
                    // the matrix must be included as a modifier of gl_Position
                    // Note that the uMVPMatrix factor *must be first* in order
                    // for the matrix multiplication product to be correct.
                    "  gl_Position = uMVPMatrix * vPosition;" +
                    "}";

        // Use to access and set the view transformation
        private int mMVPMatrixHandle;

        private readonly string fragmentShaderCode =
            "precision mediump float;" +
                    "uniform vec4 vColor;" +
                    "void main() {" +
                    "  gl_FragColor = vColor;" +
                    "}";


        private FloatBuffer vertexBuffer;

        static readonly int COORDS_PER_VERTEX = 3;
        private static readonly int numSegments = 20;
        static float[] coordinates = new float[numSegments * 3]; // in counterclockwise order:
                                                                 //         0.0f,  0.0f, 0.0f, // point 1
                                                                 //         1.0f,  0.0f, 0.0f, // point 2
                                                                 //         1.0f,  0.0f, 0.0f, // point 3
                                                                 // };

        float[] color = { 0, 0, 0, 1.0f };

        private int mPositionHandle;
        private int mColorHandle;

        private readonly int vertexCount = coordinates.Length / COORDS_PER_VERTEX;
        private readonly int vertexStride = COORDS_PER_VERTEX * 4;

        private int loadShader(int type, string shaderCode)
        {
            int shader = GLES20.GlCreateShader(type);
            GLES20.GlShaderSource(shader, shaderCode);
            GLES20.GlCompileShader(shader);
            return shader;
        }
        public void CreateOnGLThread()
        {
            try
            {
                // initialize vertex byte buffer for shape coordinates
                // number ofr coordinate values * 4 bytes per float
                ByteBuffer bb = ByteBuffer.AllocateDirect(coordinates.Length * 4);
                bb.Order(ByteOrder.NativeOrder());  // use the device hardware's native byte order
                vertexBuffer = bb.AsFloatBuffer();  // create a floating point buffer from the ByteBuffer
                vertexBuffer.Put(coordinates);  // add the coordinate to the FloatBuffer
                vertexBuffer.Position(0);   // set the buffer to read the first coordinate

                int vertexShader = loadShader(GLES20.GlVertexShader, vertexShaderCode);
                int fragmentShader = loadShader(GLES20.GlFragmentShader, fragmentShaderCode);

                mProgram = GLES20.GlCreateProgram();    // create empty OpenGL ES Program
                GLES20.GlAttachShader(mProgram, vertexShader);  // add the shader to program
                GLES20.GlAttachShader(mProgram, fragmentShader);
                GLES20.GlLinkProgram(mProgram); // create OpenGL ES program executables
                Matrix.SetIdentityM(mModelMatrix, 0);
            }
            catch { }
        }

        public void setVerts(float v0, float v1, float v2, float v3, float v4, float v5)
        {
            try
            {
                coordinates[0] = v0;
                coordinates[1] = v1;
                coordinates[2] = v2;
                coordinates[3] = v3;
                coordinates[4] = v4;
                coordinates[5] = v5;

                vertexBuffer.Put(coordinates);
                // set the buffer to read the first coordinate
                vertexBuffer.Position(0);
            }
            catch { }
        }

        public void setColor(float red, float green, float blue, float alpha)
        {
            color[0] = red;
            color[1] = green;
            color[2] = blue;
            color[3] = alpha;
        }
        private float[] mModelMatrix = new float[16];
        private float[] mModelViewMatrix = new float[16];
        private float[] mModelViewProjectionMatrix = new float[16];
        readonly string TAG = "Line";

        public void DrawLine(float[] cameraView, float[] cameraPerspective, float width = 4.0f)
        {
            try
            {
                ShaderUtil.CheckGLError(TAG, "Before draw");

                // Build the ModelView and ModelViewProjection matrices
                // for calculating object position and light.
                Matrix.MultiplyMM(mModelViewMatrix, 0, cameraView, 0, mModelMatrix, 0);
                Matrix.MultiplyMM(mModelViewProjectionMatrix, 0, cameraPerspective, 0, mModelViewMatrix, 0);

                // add program to OpenGL ES environment
                GLES20.GlUseProgram(mProgram);
                ShaderUtil.CheckGLError(TAG, "After glBindBuffer");

                GLES20.GlBindBuffer(GLES20.GlArrayBuffer, 0);
                ShaderUtil.CheckGLError(TAG, "After glBindBuffer");

                // get handle to vertex shader's vPosition member
                mPositionHandle = GLES20.GlGetAttribLocation(mProgram, "vPosition");
                ShaderUtil.CheckGLError(TAG, "After glGetAttribLocation");

                // enable a handle to the vertices
                GLES20.GlEnableVertexAttribArray(mPositionHandle);
                ShaderUtil.CheckGLError(TAG, "After glEnableVertexAttribArray");

                // prepare the coordinate data
                GLES20.GlVertexAttribPointer(mPositionHandle, COORDS_PER_VERTEX,
                        GLES20.GlFloat, false,
                        vertexStride, vertexBuffer);
                ShaderUtil.CheckGLError(TAG, "After glVertexAttribPointer");

                // get handle to fragment shader's vColor member
                mColorHandle = GLES20.GlGetUniformLocation(mProgram, "vColor");

                // set color for drawing the triangle
                GLES20.GlUniform4fv(mColorHandle, 1, color, 0);
                ShaderUtil.CheckGLError(TAG, "After glUniform4fv");

                // get handle to shape's transformation matrix
                mMVPMatrixHandle = GLES20.GlGetUniformLocation(mProgram, "uMVPMatrix");

                // Pass the projection and view transformation to the shader
                GLES20.GlUniformMatrix4fv(mMVPMatrixHandle, 1, false, mModelViewProjectionMatrix, 0);
                ShaderUtil.CheckGLError(TAG, "After glUniformMatrix4fv");

                GLES20.GlLineWidth(width);
                // Draw the line
                GLES20.GlDrawArrays(GLES20.GlLines, 0, vertexCount);
                ShaderUtil.CheckGLError(TAG, "After glDrawArrays");

                // Disable vertex array
                GLES20.GlDisableVertexAttribArray(mPositionHandle);
                ShaderUtil.CheckGLError(TAG, "After draw");
            }
            catch { }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Dispose(bool isdisposing)
        {
            if (isDisposed) return;
            if (vertexBuffer != null) vertexBuffer.Dispose();
            isDisposed = isdisposing;
        }
    }
}