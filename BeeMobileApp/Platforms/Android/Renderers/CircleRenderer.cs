using Android.Opengl;
using Java.Nio;

namespace BeeMobileApp.Platforms.Android.Renderers
{
    public class CircleRenderer : IDisposable
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
        static float[] coordinates = new float[numSegments * 3];

        private float[] mModelMatrix = new float[16];
        private float[] mModelViewMatrix = new float[16];
        private float[] mModelViewProjectionMatrix = new float[16];
        readonly string TAG = "Line";

        float[] color = { 255, 255, 255, 1f };

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

        private string Radius { get; set; }
        public void setVerts(float v0, float v1, float v2, string radius, bool IsHorizontal, float angleBwVetor)
        {
            try
            {
                double theta = 2 * Math.PI / numSegments;
                //
                //float t;
                //
                Radius = radius;
                float r = 0.02f; color[0] = 255; color[1] = 255; color[2] = 255;
                if (radius == "Point") r = 0.003f;
                else if (radius == "Diameter") { r = 0.006f; color[1] = 0; color[2] = 0; }
                float x = r;//we start at angle = 0 
                float yz = 0;

                if (IsHorizontal)
                {
                    for (int i = 0; i < numSegments * 3; i = i + 3)
                    {
                        coordinates[i] = x + v0;
                        coordinates[i + 1] = v1;
                        coordinates[i + 2] = yz + v2;

                        x = r * (float)Math.Cos(theta);//precalculate the sine and cosine
                        yz = r * (float)Math.Sin(theta);
                        theta = theta + 2 * Math.PI / numSegments;
                    }
                }
                else
                {
                    for (int i = 0; i < numSegments * 3; i = i + 3)
                    {
                        float nx = x * (float)Math.Cos(angleBwVetor);
                        float z = x * (float)Math.Sin(angleBwVetor);
                        coordinates[i] = nx + v0;
                        coordinates[i + 1] = yz + v1;
                        coordinates[i + 2] = -z + v2;

                        x = r * (float)Math.Cos(theta);//precalculate the sine and cosine
                        yz = r * (float)Math.Sin(theta);
                        theta = theta + 2 * Math.PI / numSegments;
                    }
                }
                //coordinates = v0;
                vertexBuffer.Put(coordinates);
                // set the buffer to read the first coordinate
                vertexBuffer.Position(0);
            }
            catch
            {

            }
        }

        public void setColor(float red, float green, float blue, float alpha)
        {
            color[0] = red;
            color[1] = green;
            color[2] = blue;
            color[3] = alpha;
        }

        public void DrawCircle(float[] cameraView, float[] cameraPerspective)
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

                GLES20.GlLineWidth(4.0f);
                // Draw the line
                if (Radius == "Point" || Radius == "Diameter") GLES20.GlDrawArrays(GLES20.GlTriangleFan, 0, vertexCount);
                else GLES20.GlDrawArrays(GLES20.GlLineLoop, 0, vertexCount);
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