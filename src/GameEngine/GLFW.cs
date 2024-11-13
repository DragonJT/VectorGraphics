
namespace GameEngine;
using System.Runtime.InteropServices;
using SebText.FontLoading;
using System.Numerics;

static class Kernel32{
    [DllImport("kernel32.dll")]
    public static extern void RtlZeroMemory(IntPtr dst, UIntPtr length);
}

delegate void WindowSizeCallbackDelegate(IntPtr window, int width, int height);
delegate void CursorPosCallbackDelegate(IntPtr window, double xpos, double ypos);
delegate void MouseButtonCallbackDelegate(IntPtr window, int button, int action, int mods);
delegate void KeyCallbackDelegate(IntPtr window, int key, int scancode, int action, int mods);
delegate void CharCallbackDelegate(IntPtr window, uint codepoint);

static class GLFW{
    private const string DllFilePath = @"glfw3.dll";

    public const int GLFW_OPENGL_API = 0x00030001;
    public const int GLFW_OPENGL_ES_API = 0x00030002;

    public const int GLFW_CLIENT_API = 0x00022001;
    public const int GLFW_CONTEXT_VERSION_MAJOR = 0x00022002;
    public const int GLFW_CONTEXT_VERSION_MINOR = 0x00022003;

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static int glfwInit();

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static IntPtr glfwCreateWindow(int width, int height, string title, IntPtr monitor, IntPtr share);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwMakeContextCurrent(IntPtr window);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static int glfwWindowShouldClose(IntPtr window);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwPollEvents();

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static IntPtr glfwGetProcAddress (string procname);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwWindowHint (int hint, int value);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwSwapBuffers (IntPtr window);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwGetWindowSize(IntPtr window, IntPtr width, IntPtr height);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwSetMouseButtonCallback(IntPtr window, IntPtr mouseButtonCallback);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwSetCursorPosCallback(IntPtr window, IntPtr cursorPosCallback);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwSetWindowSizeCallback(IntPtr window, IntPtr windowSizeCallback);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwGetCursorPos(IntPtr window, IntPtr xpos, IntPtr ypos);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwSetKeyCallback(IntPtr window, IntPtr keyCallback);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwSetCharCallback(IntPtr window, IntPtr charCallback);

    [DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
    public extern static void glfwGetKey(IntPtr window, int key);
}

static class GLFWHelper {
    public static void SetMouseButtonCallback(MouseButtonCallbackDelegate mouseButtonCallbackDelegate){
        var ptr = Marshal.GetFunctionPointerForDelegate(mouseButtonCallbackDelegate);
        GLFW.glfwSetMouseButtonCallback(GameEngine.window, ptr);
    }

    public static void SetCursorPosCallback(CursorPosCallbackDelegate cursorPosCallbackDelegate){
        var ptr = Marshal.GetFunctionPointerForDelegate(cursorPosCallbackDelegate);
        GLFW.glfwSetCursorPosCallback(GameEngine.window, ptr);
    }

    public static void SetKeyCallback(KeyCallbackDelegate keyCallbackDelegate){
        var ptr = Marshal.GetFunctionPointerForDelegate(keyCallbackDelegate);
        GLFW.glfwSetKeyCallback(GameEngine.window, ptr);
    }

    public static void SetWindowSizeCallback(WindowSizeCallbackDelegate windowSizeCallbackDelegate){
        var ptr = Marshal.GetFunctionPointerForDelegate(windowSizeCallbackDelegate);
        GLFW.glfwSetWindowSizeCallback(GameEngine.window, ptr);
    }

    public static void SetCharCallback(CharCallbackDelegate charCallbackDelegate){
        var ptr = Marshal.GetFunctionPointerForDelegate(charCallbackDelegate);
        GLFW.glfwSetCharCallback(GameEngine.window, ptr);
    }

    public static Vector2 GetCursorPosition(){
        var xptr = GameEngine.memory.Allocate(8);
        var yptr = GameEngine.memory.Allocate(8);
        GLFW.glfwGetCursorPos(GameEngine.window, xptr, yptr);
        return new Vector2((float)Marshal.PtrToStructure<double>(xptr), (float)Marshal.PtrToStructure<double>(yptr));
    }
}

class Shader{
    public uint program;

    static uint CreateShader(string source, uint type){
        var shader = GL.glCreateShader(type);
        GL.glShaderSource(shader, 1, GameEngine.memory.AddStringArray([source]), 0);
        GL.glCompileShader(shader);
        var success = GameEngine.memory.Allocate(4);
        GL.glGetShaderiv(shader, GL.GL_COMPILE_STATUS, success);
        if(Marshal.PtrToStructure<int>(success) == 0){
            var infoLog = GameEngine.memory.Allocate(512);
            GL.glGetShaderInfoLog(shader, 512, 0, infoLog);
            throw new Exception(Marshal.PtrToStringAnsi(infoLog));
        }
        return shader;
    }

    public Shader(string vertexSource, string fragmentSource){
        var vertexShader = CreateShader(vertexSource, GL.GL_VERTEX_SHADER);
        var fragmentShader = CreateShader(fragmentSource, GL.GL_FRAGMENT_SHADER);
        program = GL.glCreateProgram();
        GL.glAttachShader(program, vertexShader);
        GL.glAttachShader(program, fragmentShader);
        GL.glLinkProgram(program);
        var success = GameEngine.memory.Allocate(4);
        GL.glGetProgramiv(program, GL.GL_LINK_STATUS, success);
        if(Marshal.PtrToStructure<int>(success) == 0) {
            var infoLog = GameEngine.memory.Allocate(512);
            GL.glGetProgramInfoLog(program, 512, 0, infoLog);
            throw new Exception(Marshal.PtrToStringAnsi(infoLog));
        }
        GL.glDeleteShader(vertexShader);
        GL.glDeleteShader(fragmentShader);  
    }

    public void Use(){
        GL.glUseProgram(program);
    }
}

class Buffer(int maxSize){
    public readonly IntPtr ptr = Marshal.AllocHGlobal(maxSize);
    public readonly int maxSize = maxSize;
    public int size = 0;

    public void SetAllDataToZero(){
        Kernel32.RtlZeroMemory(ptr, (uint)maxSize);
    }

    public byte[] GetBytes(int id, int size){
        byte[] managedArray = new byte[size];
        Marshal.Copy(ptr + id, managedArray, 0, size);
        return managedArray;
    }

    public void SetBytes(int id, byte[] bytes){
        Marshal.Copy(bytes, 0, ptr + id, bytes.Length);
    }

    public IntPtr AddBytes(byte[] bytes){
        var ptr = Allocate(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        return ptr;
    }

    public IntPtr AddFloatArray(float[] array){
        return AddBytes(array.SelectMany(BitConverter.GetBytes).ToArray());
    }

    public IntPtr AddString(string @string){
        byte[] bytes = @string.Select(c=>(byte)c).ToArray();
        return AddBytes(bytes);
    }

    public IntPtr AddStringArray(string[] strings){
        IntPtr[] ptrs = strings.Select(AddString).ToArray();
        return AddBytes(ptrs.SelectMany(i=>BitConverter.GetBytes(i)).ToArray());
    }

    public IntPtr AddIntArray(int[] array){
        return AddBytes(array.SelectMany(BitConverter.GetBytes).ToArray());
    }

    public IntPtr AddUintArray(uint[] array){
        return AddBytes(array.SelectMany(BitConverter.GetBytes).ToArray());
    }

    public IntPtr Allocate(int size){
        if(this.size + size > maxSize){
            throw new Exception("Buffer not large enough...");
        }
        var ptr = this.ptr + this.size;
        this.size += size;
        return ptr;
    }

    public void FreeAll(){
        Marshal.FreeHGlobal(ptr);
    }
}

static class GLHelper{
    public static uint GenVertexArray(){
        var ptr = GameEngine.memory.Allocate(4);
        GL.glGenVertexArrays(1, ptr);
        return Marshal.PtrToStructure<uint>(ptr);
    }

    public static uint GenBuffer(){
        var ptr = GameEngine.memory.Allocate(4);
        GL.glGenBuffers(1, ptr);
        return Marshal.PtrToStructure<uint>(ptr);
    }

    public static uint GenTexture(){
        var ptr = GameEngine.memory.Allocate(4);
        GL.glGenTextures(1, ptr);
        return Marshal.PtrToStructure<uint>(ptr);
    }

    public static void UniformMatrix4fv(Shader shader, string name, Matrix4x4 matrix){
        var namePtr = GameEngine.memory.AddString(name+'\0');
        var location = GL.glGetUniformLocation(shader.program, namePtr);
        var matrixPtr = GameEngine.memory.AddFloatArray([
            matrix.M11, matrix.M12, matrix.M13, matrix.M14, 
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34, 
            matrix.M41, matrix.M42, matrix.M43, matrix.M44,
        ]);
        GL.glUniformMatrix4fv(location, 1, 0, matrixPtr);
    }
}

class DynamicTextureRed {
    uint id;
    public readonly int width;
    public readonly int height;
    Buffer data;

    public DynamicTextureRed(int width, int height){
        id = GLHelper.GenTexture();
        this.width = width;
        this.height = height;
        data = new Buffer(width * height);
        data.SetAllDataToZero();
    }

    public void UpdateData(byte[] bytes){
        data.SetBytes(0, bytes);
        GL.glBindTexture(GL.GL_TEXTURE_2D, id);

        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, (int)GL.GL_REPEAT);	
        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, (int)GL.GL_REPEAT);
        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, (int)GL.GL_NEAREST);
        GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, (int)GL.GL_NEAREST);

        GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, (int)GL.GL_RED, width, height, 0, GL.GL_RED, GL.GL_UNSIGNED_BYTE, data.ptr);
    }

    public void Bind(){
        GL.glBindTexture(GL.GL_TEXTURE_2D, id);
    }
}

class Renderer(Shader shader, Buffer vertices, Buffer indices){
    readonly uint vao = GLHelper.GenVertexArray();
    readonly uint vbo = GLHelper.GenBuffer();
    readonly uint ebo = GLHelper.GenBuffer();
    public readonly Buffer vertices = vertices, indices = indices;
    readonly Shader shader = shader;

    public void UpdateData(){
        GL.glBindVertexArray(vao);

        GL.glBindBuffer(GL.GL_ARRAY_BUFFER, vbo);
        GL.glBufferData(GL.GL_ARRAY_BUFFER, vertices.size, vertices.ptr, GL.GL_DYNAMIC_DRAW);

        GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, ebo);
        GL.glBufferData(GL.GL_ELEMENT_ARRAY_BUFFER, indices.size, indices.ptr, GL.GL_DYNAMIC_DRAW);
    }

    public void SetVertexAttribPointers(int[] vertexAttribPointerSizes){
        GL.glBindVertexArray(vao);
        var stride = 0;
        foreach(var v in vertexAttribPointerSizes){
            stride+=v;
        }
        uint id = 0;
        var offset = 0;
        foreach(var v in vertexAttribPointerSizes){
            GL.glVertexAttribPointer(id, v, GL.GL_FLOAT, GL.GL_FALSE, stride * sizeof(float), offset * sizeof(float));
            GL.glEnableVertexAttribArray(id);
            offset += v;
            id++;
        }
    }

    public void UseShader(){
        shader.Use();
    }

    public void SetMatrix(string name, Matrix4x4 matrix){
        GLHelper.UniformMatrix4fv(shader, name, matrix);
    }

    public void Draw(){
        GL.glBindVertexArray(vao);
        GL.glDrawElements(GL.GL_TRIANGLES, indices.size / sizeof(uint), GL.GL_UNSIGNED_INT, 0);
    }

    public void ClearData(){
        vertices.size = 0;
        indices.size = 0;
    }
}

class DynamicTextureRenderer2D {
    public int Width => dynamicTextureRed.width;
    public int Height => dynamicTextureRed.height;
    Renderer renderer;
    DynamicTextureRed dynamicTextureRed;
    uint vertexID;

    public DynamicTextureRenderer2D(int width, int height){
        string vertexSource = @"#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec2 aTexCoord;

uniform mat4 projection;

out vec4 color;
out vec2 TexCoord;

void main()
{
    gl_Position = projection * vec4(aPos, 0.0, 1.0);
    color = aColor;
    TexCoord = aTexCoord;
}  "+"\0";

        string fragmentSource = @"#version 330 core
out vec4 FragColor;
  
in vec4 color;
in vec2 TexCoord;

uniform sampler2D ourTexture;

void main()
{
    float c = texture(ourTexture, TexCoord).r;
    FragColor = vec4(c, c, c, c) * color;
} "+"\0";
        var shader = new Shader(vertexSource, fragmentSource);
        var vertices = new Buffer(sizeof(float) * 100000);
        var indices = new Buffer(sizeof(uint) * 30000);
        dynamicTextureRed = new DynamicTextureRed(width, height);
        renderer = new Renderer(shader, vertices, indices);
    }

    public void UpdateTexture(byte[] bytes){
        dynamicTextureRed.UpdateData(bytes);
    }

    public void DrawPoly(Vector2[] points, Vector2[] uvs, Color color){
        for(uint i=2;i<points.Length;i++){
            renderer.indices.AddUintArray([vertexID, vertexID+i-1, vertexID+i]);
        }
        for(var i=0;i<points.Length;i++){
            renderer.vertices.AddFloatArray([points[i].x, points[i].y, color.r, color.g, color.b, color.a, uvs[i].x, uvs[i].y]);
            vertexID++;
        }
    }

    public void DrawMesh(Mesh2D mesh, Vector2 uv, Color color){
        renderer.indices.AddUintArray(mesh.triangles.Select(t=>t+vertexID).ToArray());
        for(var i=0;i<mesh.vertices.Count;i++){
            renderer.vertices.AddFloatArray([mesh.vertices[i].x, mesh.vertices[i].y, color.r, color.g, color.b, color.a, uv.x, uv.y]);
            vertexID++;
        }
    }

    public void Draw(){
        GL.glEnable(GL.GL_BLEND); 
        GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA); 
        renderer.UseShader();
        renderer.UpdateData();
        renderer.SetMatrix("projection", Matrix4x4.CreateOrthographicOffCenter(0, Screen.width, Screen.height, 0, -1, 1));
        renderer.SetVertexAttribPointers([2,4,2]);
        dynamicTextureRed.Bind();
        renderer.Draw();
        renderer.ClearData();
        vertexID = 0;
    }
}

class CharacterData(char c, Vector2 uvMin, Vector2 uvMax, FontData.GlyphData glyphData){
    public char c = c;
    public Vector2 uvMin = uvMin;
    public Vector2 uvMax = uvMax;
    public FontData.GlyphData glyphData = glyphData;
}

class FontRenderer{
    byte[] bytes;
    int width;
    int height;
    FontData fontData;
    float fontScale;
    Dictionary<char, CharacterData> characterData = [];
    DynamicTextureRenderer2D dynamicTextureRenderer2D;

    public float FontHeight(float characterScale){
        return 2000 * fontScale * characterScale;
    }

    public FontRenderer(string pathToFont, int textureSize, float fontScale){
        fontData = FontParser.Parse(pathToFont);
        dynamicTextureRenderer2D = new DynamicTextureRenderer2D(textureSize, textureSize);
        width = textureSize;
        height = textureSize;
        bytes = new byte[width * height];
        this.fontScale = fontScale;
        int x = 0;
        int y = (int)FontHeight(1);
        var str = "";
        for(var i=33;i<128;i++){
            str+=(char)i;
        }
        SetPixel(0,0,255);
        DrawTextOntoTexture(ref x, ref y, str);
        dynamicTextureRenderer2D.UpdateTexture(bytes);
    }

    void SetPixel(int x, int y, byte value){
        bytes[x + y * width] = value;
    }

    byte GetPixel(int x, int y){
        return bytes[x + y * width];
    }

    void DrawCharacterOntoTexture(ref int posX, ref int posY, char c){
        if(fontData.TryGetGlyph(c, out FontData.GlyphData glyphData)){
            var minY = (int)(posY + glyphData.MinY * fontScale);
            var minX = (int)(posX + glyphData.MinX * fontScale);
            var maxY = (int)(posY + glyphData.MaxY * fontScale);
            var maxX = (int)(posX + glyphData.MaxX * fontScale);
            if(minX < 0 || maxX >= width || minY < 0 || maxY >= height){
                posX = 0;
                posY += (int)(fontScale * 2200);
                minY = (int)(posY + glyphData.MinY * fontScale);
                minX = (int)(posX + glyphData.MinX * fontScale);
                maxY = (int)(posY + glyphData.MaxY * fontScale);
                maxX = (int)(posX + glyphData.MaxX * fontScale);
            }
            var uvMin = new Vector2(minX/(float)width, minY/(float)height);
            var uvMax = new Vector2(maxX/(float)width, maxY/(float)height);
            characterData.Add(c, new CharacterData(c, uvMin, uvMax, glyphData));
            List<Vector2[]> contours = GlythHelper.CreateContoursWithImpliedPoints(glyphData, fontScale);
            for(var ci = 0; ci< contours.Count; ci++){
                var contour = contours[ci];
                for(var i = 0;i < contour.Length-2; i+=2){
                    var dist = (contour[i] - contour[i+2]).Length();
                    int resolution = (int)(dist * 2);
                    for(var ti=0;ti<=resolution;ti++){
                        var point = Vector2.Bezier(contour[i], contour[i+1], contour[i+2], ti/(float)resolution);
                        var pointI = new Vector2i((int)point.x, (int)point.y);
                        if(contour[i].y < contour[i+2].y){
                            if(GetPixel(posX + pointI.x, posY + pointI.y) == 0){
                                SetPixel(posX + pointI.x, posY + pointI.y, 254);
                            }
                        }
                        else{
                            SetPixel(posX + pointI.x, posY + pointI.y, 253);
                        }
                    }
                }
            }
            for(var y=minY;y<maxY;y++){
                bool draw = false;
                for(var x=minX;x<maxX;x++){
                    if(GetPixel(x,y) == 254){
                        draw = true;
                    }
                    else if(GetPixel(x,y) == 253){
                        draw = false;
                    }
                    else if(GetPixel(x, y-1) == 255){
                        SetPixel(x,y,255);
                    }
                    else if(draw){
                        SetPixel(x,y,255);
                    }
                }
            }
            posX += (int)(glyphData.AdvanceWidth * fontScale);
        }
    }

    void DrawTextOntoTexture(ref int x, ref int y, string text){
        for(var i=0;i<text.Length;i++){
            DrawCharacterOntoTexture(ref x,ref y, text[i]);
        }
    }

    public void DrawMesh(Mesh2D mesh, Color color) {
        Vector2 uv = new (0.5f/width,0.5f/height);
        dynamicTextureRenderer2D.DrawMesh(mesh, uv, color);
    }

    public float DrawCharacter(Vector2 position, char c, float characterScale, Color color){
        if(c == ' '){
            return FontHeight(characterScale) * 0.5f;
        }
        var character = characterData[c];
        var minX = character.glyphData.MinX * fontScale * characterScale;
        var minY = character.glyphData.MinY * fontScale * characterScale;
        var w = character.glyphData.Width * fontScale * characterScale;
        var h = character.glyphData.Height * fontScale * characterScale;
        var fontHeight = FontHeight(characterScale);

        Vector2[] points = [
            new Vector2(position.x + minX, position.y + fontHeight - minY), 
            new Vector2(position.x + minX + w, position.y + fontHeight - minY), 
            new Vector2(position.x + minX + w, position.y + fontHeight - minY - h), 
            new Vector2(position.x + minX, position.y + fontHeight - minY - h)];
        Vector2[] uvs = [
            character.uvMin, 
            new Vector2(character.uvMax.x, character.uvMin.y), 
            character.uvMax,
            new Vector2(character.uvMin.x, character.uvMax.y)];
        dynamicTextureRenderer2D.DrawPoly(points, uvs, color);
        return character.glyphData.AdvanceWidth * fontScale * characterScale;
    }

    public float DrawText(Vector2 position, string text, float characterScale, Color color){
        var x = 0f;
        foreach(var c in text){
            x += DrawCharacter(new Vector2(position.x + x, position.y), c, characterScale, color);
        }
        return x;
    }

    public float MeasureCharacter(char c, float characterScale){
        if(c == ' '){
            return FontHeight(characterScale) * 0.5f;
        }
        else if(characterData.TryGetValue(c, out CharacterData? character)){
            return character.glyphData.AdvanceWidth * fontScale * characterScale;
        }
        return 0;
    }

    public float MeasureText(string text, float characterScale){
        var x = 0f;
        foreach(var c in text){
            x += MeasureCharacter(c, characterScale);
        }
        return x;
    }

    public void Draw(){
        dynamicTextureRenderer2D.Draw();
    }
}
