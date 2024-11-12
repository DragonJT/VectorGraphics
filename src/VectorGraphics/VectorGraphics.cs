namespace VectorGraphics;

using System.Runtime.InteropServices.Marshalling;
using GameEngine;

abstract class Component {
    public required Entity entity;

    public virtual void Draw(){}

    public T? GetComponent<T>() where T:Component{
        return entity.GetComponent<T>();
    }
}

class Transform : Component {
    public Vector2 position;
    //public float angle;
}

enum Shape { Rectangle, Ellipse }

class ShapeRenderer : Component {
    public Vector2 size;
    public Color color;
    public Shape shape;

    public override void Draw(){
        var transform = GetComponent<Transform>()!;
        var rect = Rect.CreateFromCenterSize(transform.position, size);
        if(shape == Shape.Rectangle){
            Graphics.DrawRect(rect, color);
        }
        else if(shape == Shape.Ellipse){
            Graphics.DrawEllipse(rect, 32, color);
        }
    }
}

class TextRenderer : Component {
    public string text = "";
    public float size = 0.5f;
    public Color color = Color.Black;

    public override void Draw(){
        var transform = GetComponent<Transform>()!;
        var width = Graphics.MeasureText(text, size);
        var x = transform.position.x - width / 2f;
        var y = transform.position.y - Graphics.FontHeight(size) / 2f;
        Graphics.DrawText(new Vector2(x, y), text, size, color);
    }
}

class Entity {
    public readonly List<Component> components = [];

    public T AddComponent<T>() where T:Component {
        var component = Activator.CreateInstance<T>();
        component.entity = this;
        components.Add(component);
        return component;
    }

    public T? GetComponent<T>() where T:Component {
        return components.OfType<T>().FirstOrDefault();
    }
}

class VectorGraphics : Game {
    bool dragging = false;
    Vector2 start;
    Vector2 end;
    List<Entity> entities = [];
    List<Entity> selected = [];
    Color color = Color.Blue;
    Color fontColor = Color.Black;
    float fontSize = 0.5f;
    string tool = "Rect";

    void CreateShape(Vector2 position, Shape shapeType, Color color){
        var entity = new Entity();
        var transform = entity.AddComponent<Transform>();
        transform.position = position;
        var shapeRenderer = entity.AddComponent<ShapeRenderer>();
        shapeRenderer.shape = shapeType;
        shapeRenderer.color = color;
        entities.Add(entity);
        selected.Clear();
        selected.Add(entity);
    }

    void UpdateShape(Entity entity){
        var rect = Rect.CreateFromStartEnd(start, end);
        var transform = entity.GetComponent<Transform>()!;
        transform.position = rect.Center;
        var shapeRenderer = entity.GetComponent<ShapeRenderer>()!;
        shapeRenderer.size = rect.Size;
    }

    public override void MouseButtonCallback(int button, int action, int mods){
        if(ImGUI.mouseOver){
            return;
        }
        if(tool == "Rect" || tool == "Ellipse"){
            if(button == Input.MOUSE_BUTTON_1 && action == Input.PRESS){
                start = Input.MousePosition;
                end = Input.MousePosition;
                dragging = true;
                if(tool == "Rect"){
                    CreateShape(start, Shape.Rectangle, color);
                }
                else if(tool == "Ellipse"){
                    CreateShape(start, Shape.Ellipse, color);
                }
            }
            if(button == Input.MOUSE_BUTTON_1 && action == Input.RELEASE){
                dragging = false;
            }
        }
        else if(tool == "Edit"){
            if(button == Input.MOUSE_BUTTON_1 && action == Input.PRESS){
                
            }
        }
    }

    public override void KeyCallback(int key, int scancode, int action, int mods){
        if(key == Input.KEY_BACKSPACE && (action == Input.PRESS || action == Input.REPEAT)){
            foreach(var entity in selected){
                var textRenderer = entity.GetComponent<TextRenderer>();
                if(textRenderer!=null && textRenderer.text.Length > 0){
                    textRenderer.text = textRenderer.text[0..^1];
                }
                else{
                    entities.Remove(entity);
                }
            }
        }
    }

    public override void CharCallback(uint codepoint){
        if(codepoint >= 32 && codepoint < 128){
            if(selected.Count > 0){
                foreach(var s in selected){
                    var textRenderer = s.GetComponent<TextRenderer>();
                    textRenderer ??= s.AddComponent<TextRenderer>();
                    textRenderer.color = fontColor;
                    textRenderer.size = fontSize;
                    textRenderer.text += (char)codepoint;
                }
            }
        }
    }

    public override void Draw(){
        if(dragging){
            end = Input.MousePosition;
            foreach(var e in selected){
                UpdateShape(e);
            }
        }
        Graphics.Clear(Color.White);
        foreach(var entity in entities){
            foreach(var c in entity.components){
                c.Draw();
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