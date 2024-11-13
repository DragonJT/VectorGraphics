namespace VectorGraphics;

using GameEngine;

class VGText {
    public string text = "";
    public float size = 0.5f;
    public Color color = Color.Black;

    public void Draw(Vector2 center){
        var width = Graphics.MeasureText(text, size);
        var x = center.x - width / 2f;
        var y = center.y - Graphics.FontHeight(size) / 2f;
        Graphics.DrawText(new Vector2(x, y), text, size, color);
    }
}

class VGMesh(Mesh2D mesh, Color color) {
    public Mesh2D mesh = mesh;
    public Color color = color;
    public VGText? text;
    public bool selected = true;
    
    public void Draw(){
        Graphics.Draw(mesh, color);
        text?.Draw(mesh.Center);
    }
}

class VectorGraphics : Game {
    bool dragging = false;
    Vector2 start;
    Vector2 end;
    List<VGMesh> meshes = [];
    Color color = Color.Blue;
    Color fontColor = Color.Black;
    float fontSize = 0.5f;
    string tool = "Rect";

    void ClearSelected(){
        foreach(var m in meshes){
            m.selected = false;
        }
    }

    public override void MouseButtonCallback(int button, int action, int mods){
        if(ImGUI.mouseOver){
            return;
        }
        if(tool == "Rect" || tool == "Ellipse"){
            if(button == Input.MOUSE_BUTTON_1 && action == Input.PRESS){
                ClearSelected();
                start = Input.MousePosition;
                end = Input.MousePosition;
                dragging = true;
                if(tool == "Rect"){
                    meshes.Add(new VGMesh(Mesh2D.Rect(Rect.CreateFromStartEnd(start, end)), color));
                }
                else if(tool == "Ellipse"){
                    meshes.Add(new VGMesh(Mesh2D.Ellipse(Rect.CreateFromStartEnd(start, end), 32), color));
                }
            }
            if(button == Input.MOUSE_BUTTON_1 && action == Input.RELEASE){
                dragging = false;
            }
        }
        else if(tool == "Edit"){
            if(button == Input.MOUSE_BUTTON_1 && action == Input.PRESS){
                ClearSelected();
                for(var i = meshes.Count-1;i>=0;i--){
                    if(meshes[i].mesh.Contains(Input.MousePosition)){
                        meshes[i].selected = true;
                        break;
                    }
                }
            }
        }
    }

    public override void KeyCallback(int key, int scancode, int action, int mods){
        if(key == Input.KEY_BACKSPACE && (action == Input.PRESS || action == Input.REPEAT)){
            foreach(var s in meshes.Where(m=>m.selected).ToArray()){
                var vgtext = s.text;
                if(vgtext!=null){ 
                    if(vgtext.text.Length > 0){
                        vgtext.text = vgtext.text[0..^1];
                    }
                    else{
                        s.text = null;
                    }
                }
                else {
                    meshes.Remove(s);
                }
            }
        }
    }

    public override void CharCallback(uint codepoint){
        if(codepoint >= 32 && codepoint < 128){
            foreach(var s in meshes.Where(m=>m.selected)){
                var textRenderer = s.text;
                textRenderer ??= s.text = new VGText();
                textRenderer.color = fontColor;
                textRenderer.size = fontSize;
                textRenderer.text += (char)codepoint;
            }
        }
    }

    public override void Draw(){
        if(dragging){
            end = Input.MousePosition;
            foreach(var m in meshes.Where(m=>m.selected)){
                if(tool == "Rect"){
                    m.mesh = Mesh2D.Rect(Rect.CreateFromStartEnd(start, end));
                }
                else if(tool == "Ellipse"){
                    m.mesh = Mesh2D.Ellipse(Rect.CreateFromStartEnd(start, end), 32);
                }
            }
        }
        if(tool == "Edit" && Input.GetButton(Input.MOUSE_BUTTON_1)){
            foreach(var m in meshes.Where(m=>m.selected)){
                m.mesh.Translate(Input.DeltaMousePosition);
            }
        }

        Graphics.Clear(Color.White);
        foreach(var vgmesh in meshes){
            vgmesh.Draw();
        }
        foreach(var s in meshes.Where(m=>m.selected)){
            Graphics.Draw(Mesh2D.RectBorder(s.mesh.GetBounds(), 5), Color.LightCyan);
        }
        ImGUI.Start(new Rect(0,0,300,Screen.height));
        color.r = ImGUI.Slider("R", color.r);
        color.g = ImGUI.Slider("G", color.g);
        color.b = ImGUI.Slider("B", color.b);
        color.a = ImGUI.Slider("A", color.a);
        tool = ImGUI.Options(tool, ["Edit", "Rect", "Ellipse", "Poly"]);
        fontColor.r = ImGUI.Slider("FontR", fontColor.r);
        fontColor.g = ImGUI.Slider("FontG", fontColor.g);
        fontColor.b = ImGUI.Slider("FontB", fontColor.b);
        fontColor.a = ImGUI.Slider("FontA", fontColor.a);
        fontSize = ImGUI.Slider("FontSize", fontSize);
    }

    static void Main(){
        GameEngine.Create(new VectorGraphics());
    }
}