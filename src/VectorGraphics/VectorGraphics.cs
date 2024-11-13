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

static class Handles {
    static string? id;
    const float handleSize = 25;

    static bool PositionHandle(string name, Vector2 position, float radius){
        var handleRect = Rect.CreateFromCenterSize(position, new Vector2(radius, radius));
        var color = id == name ? Color.White : Color.LightCyan;
        Graphics.Draw(Mesh2D.Rect(handleRect), color);
        Graphics.Draw(Mesh2D.RectBorder(handleRect, 2), Color.Black);
        if(Input.GetButtonDown(Input.MOUSE_BUTTON_1) && handleRect.Contains(Input.MousePosition)) {
            id = name;
            return true;
        }
        if(Input.GetButtonUp(Input.MOUSE_BUTTON_1) && id == name){
            id = null;
            return true;
        }
        if(Input.GetButton(Input.MOUSE_BUTTON_1) && id == name){
            return true;
        }
        return false;
    }

    public static bool RectHandle(Mesh2D mesh){
        var rect = mesh.GetBounds();
        var minx = rect.x;
        var maxx = rect.x + rect.width;
        var miny = rect.y;
        var maxy = rect.y + rect.height;
        var newMinX = minx;
        var newMaxX = maxx;
        var newMinY = miny;
        var newMaxY = maxy;
        var used = false;
        Graphics.Draw(Mesh2D.RectBorder(rect, 5), Color.LightCyan);
        if(PositionHandle("TopLeft", new Vector2(minx, miny), handleSize)){
            newMinX += Input.DeltaMousePosition.x;
            newMinY += Input.DeltaMousePosition.y;
            used = true;
        }
        if(PositionHandle("TopRight", new Vector2(maxx, miny), handleSize)){
            newMaxX += Input.DeltaMousePosition.x;
            newMinY += Input.DeltaMousePosition.y;
            used = true;
        }
        if(PositionHandle("BottomRight", new Vector2(maxx, maxy), handleSize)){
            newMaxX += Input.DeltaMousePosition.x;
            newMaxY += Input.DeltaMousePosition.y;
            used = true;
        }
        if(PositionHandle("BottomLeft", new Vector2(minx, maxy), handleSize)){
            newMinX += Input.DeltaMousePosition.x;
            newMaxY += Input.DeltaMousePosition.y;
            used = true;
        }
        if(used == true){
            var newRect = new Rect(newMinX, newMinY, newMaxX - newMinX, newMaxY - newMinY);
            for(var i = 0;i < mesh.vertices.Count;i++) {
                mesh.vertices[i] = newRect.FromFraction(rect.Fraction(mesh.vertices[i]));
            }
            return true;
        }
        return false;
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
        var selected = meshes.FirstOrDefault(m=>m.selected);
        if(dragging){
            end = Input.MousePosition;
            if(selected!=null){
                if(tool == "Rect"){
                    selected.mesh = Mesh2D.Rect(Rect.CreateFromStartEnd(start, end));
                }
                else if(tool == "Ellipse"){
                    selected.mesh = Mesh2D.Ellipse(Rect.CreateFromStartEnd(start, end), 32);
                }
            }
        }

        Graphics.Clear(Color.White);
        foreach(var vgmesh in meshes){
            vgmesh.Draw();
        }
        if(tool == "Edit"){
            bool rectHandleUsed = false;
            if(selected != null){
                rectHandleUsed = Handles.RectHandle(selected.mesh);
            }
            if(!rectHandleUsed){
                if(Input.GetButtonDown(Input.MOUSE_BUTTON_1)){
                    ClearSelected();
                    for(var i = meshes.Count-1;i>=0;i--){
                        if(meshes[i].mesh.Contains(Input.MousePosition)){
                            meshes[i].selected = true;
                            break;
                        }
                    }
                }
                else if(Input.GetButton(Input.MOUSE_BUTTON_1)){
                    selected?.mesh.Translate(Input.DeltaMousePosition);
                }
            }
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