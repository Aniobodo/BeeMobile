using Android.Content;
using Android.Graphics;
using Android.Opengl;
using Android.Util;
using Google.AR.Core;
using Java.Lang;
using Java.Nio;
using Matrix = Android.Opengl.Matrix;


namespace BeeMobileApp.Platforms.Android.Renderers
{
    public class ARImageRenderer : IDisposable
    {
        bool isDisposed;
        private readonly string TAG = "ImageRenderer";
        private int[] mTextures = new int[1];
        private static readonly float[] QUAD_COORDS = new float[12]; //{
        //         // x, y, z
        //    -.1f, 0.0f, -.2f, 
        //    -.1f, 0.0f, +.1f, 
        //    +.1f, 0.0f, -.2f, 
        //    +.1f, 0.0f, +.1f, 
        //};
        private static readonly float[] QUAD_TEXCOORDS = new float[] {
          // x, y
            0.0f, 1.0f,
            0.0f, 0.0f,
            1.0f, 1.0f,
            1.0f, 0.0f,
         };
        private static readonly int COORDS_PER_VERTEX = 3;
        private static readonly int TEXCOORDS_PER_VERTEX = 2;

        private FloatBuffer mQuadVertices;
        private FloatBuffer mQuadTexCoord;
        private int mQuadProgram;
        private int mQuadPositionParam;
        private int mQuadTexCoordParam;
        private int mTextureUniform;
        private int mModelViewProjectionUniform;

        // Temporary matrices allocated here to reduce number of allocations for each frame.
        private float[] mModelMatrix = new float[16];
        private float[] mModelViewMatrix = new float[16];
        private float[] mModelViewProjectionMatrix = new float[16];

        public void CreateOnGlThread(Context context, byte[] ImgName)
        {
            // Read the texture.
            Bitmap textureBitmap;
            try
            {
                Stream stream = new MemoryStream(ImgName);
                textureBitmap = BitmapFactory.DecodeStream(stream);
            }
            catch (IOException e)
            {
                Log.Error(TAG, "Exception reading texture", e);
                return;
            }
            int vertexShader = ShaderUtil.LoadGLShader(TAG, context, GLES20.GlVertexShader, _Microsoft.Android.Resource.Designer.ResourceConstant.Raw.Image_vertex);
            int fragmentShader = ShaderUtil.LoadGLShader(TAG, context, GLES20.GlFragmentShader, _Microsoft.Android.Resource.Designer.ResourceConstant.Raw.Image_fragment);

            GLES20.GlActiveTexture(GLES20.GlTexture0);
            GLES20.GlGenTextures(mTextures.Length, mTextures, 0);
            GLES20.GlBindTexture(GLES20.GlTexture2d, mTextures[0]);

            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlNearest);
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlNearest);
            GLUtils.TexImage2D(GLES20.GlTexture2d, 0, textureBitmap, 0);
            GLES20.GlBindTexture(GLES20.GlTexture2d, 0);

            textureBitmap.Recycle();

            ShaderUtil.CheckGLError(TAG, "Texture loading");

            // Build the geometry of a simple quad.

            int numVertices = 4;
            if (numVertices != QUAD_COORDS.Length / COORDS_PER_VERTEX)
            {
                throw new AndroidRuntimeException("Unexpected number of vertices in BackgroundRenderer.");
            }

            ByteBuffer bbVertices = ByteBuffer.AllocateDirect(QUAD_COORDS.Length * Float.Bytes);
            bbVertices.Order(ByteOrder.NativeOrder());
            mQuadVertices = bbVertices.AsFloatBuffer();
            mQuadVertices.Put(QUAD_COORDS);
            mQuadVertices.Position(0);

            ByteBuffer bbTexCoords = ByteBuffer.AllocateDirect(numVertices * TEXCOORDS_PER_VERTEX * Float.Bytes);
            bbTexCoords.Order(ByteOrder.NativeOrder());
            mQuadTexCoord = bbTexCoords.AsFloatBuffer();
            mQuadTexCoord.Put(QUAD_TEXCOORDS);
            mQuadTexCoord.Position(0);

            ByteBuffer bbTexCoordsTransformed = ByteBuffer.AllocateDirect(numVertices * TEXCOORDS_PER_VERTEX * Float.Bytes);
            bbTexCoordsTransformed.Order(ByteOrder.NativeOrder());


            //int vertexShader = loadGLShader(TAG, GLES20.GlVertexShader, VERTEX_SHADER);
            //int fragmentShader = loadGLShader(TAG,GLES20.GlFragmentShader, FRAGMENT_SHADER);

            mQuadProgram = GLES20.GlCreateProgram();
            GLES20.GlAttachShader(mQuadProgram, vertexShader);
            GLES20.GlAttachShader(mQuadProgram, fragmentShader);
            GLES20.GlLinkProgram(mQuadProgram);
            GLES20.GlUseProgram(mQuadProgram);

            ShaderUtil.CheckGLError(TAG, "Program creation");

            mQuadPositionParam = GLES20.GlGetAttribLocation(mQuadProgram, "a_Position");
            mQuadTexCoordParam = GLES20.GlGetAttribLocation(mQuadProgram, "a_TexCoord");
            mTextureUniform = GLES20.GlGetUniformLocation(mQuadProgram, "u_Texture");
            mModelViewProjectionUniform =
                GLES20.GlGetUniformLocation(mQuadProgram, "u_ModelViewProjection");

            ShaderUtil.CheckGLError(TAG, "Program parameters");

            Matrix.SetIdentityM(mModelMatrix, 0);


        }

        public void setVerts(List<List<float>> coords, Pose InitPoint, float angle)
        {
            int ind = 0;
            foreach (var coord in coords)
            {
                QUAD_COORDS[ind] = coord[0] * (float)System.Math.Cos(angle) - coord[1] * (float)System.Math.Sin(angle) + InitPoint.Tx();
                QUAD_COORDS[ind + 1] = InitPoint.Ty();
                QUAD_COORDS[ind + 2] = InitPoint.Tz() - (coord[0] * (float)System.Math.Sin(angle) + coord[1] * (float)System.Math.Cos(angle));
                ind += 3;
            }
            mQuadVertices.Put(QUAD_COORDS);
            mQuadVertices.Position(0);
        }

        public void DrawImage(float[] cameraView, float[] cameraPerspective)
        {
            ShaderUtil.CheckGLError(TAG, "Before draw");
            Matrix.MultiplyMM(mModelViewMatrix, 0, cameraView, 0, mModelMatrix, 0);
            Matrix.MultiplyMM(mModelViewProjectionMatrix, 0, cameraPerspective, 0, mModelViewMatrix, 0);



            GLES20.GlUseProgram(mQuadProgram);

            // Attach the object texture.
            GLES20.GlActiveTexture(GLES20.GlTexture0);
            GLES20.GlBindTexture(GLES20.GlTexture2d, mTextures[0]);
            GLES20.GlUniform1i(mTextureUniform, 0);
            GLES20.GlUniformMatrix4fv(mModelViewProjectionUniform, 1, false, mModelViewProjectionMatrix, 0);
            // Set the vertex positions.
            GLES20.GlVertexAttribPointer(mQuadPositionParam, COORDS_PER_VERTEX, GLES20.GlFloat, false, 0, mQuadVertices);

            // Set the texture coordinates.
            GLES20.GlVertexAttribPointer(mQuadTexCoordParam, TEXCOORDS_PER_VERTEX, GLES20.GlFloat, false, 0, mQuadTexCoord);

            // Enable vertex arrays
            GLES20.GlEnableVertexAttribArray(mQuadPositionParam);
            GLES20.GlEnableVertexAttribArray(mQuadTexCoordParam);

            GLES20.GlDrawArrays(GLES20.GlTriangleStrip, 0, 4);

            // Disable vertex arrays
            GLES20.GlDisableVertexAttribArray(mQuadPositionParam);
            GLES20.GlDisableVertexAttribArray(mQuadTexCoordParam);

            GLES20.GlBindTexture(GLES20.GlTexture2d, 0);

            ShaderUtil.CheckGLError(TAG, "After draw");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Dispose(bool isdisposing)
        {
            if (isDisposed) return;
            if (mQuadVertices != null) mQuadVertices.Dispose();
            if (mQuadTexCoord != null) mQuadTexCoord.Dispose();
            isDisposed = isdisposing;
        }
    }
}