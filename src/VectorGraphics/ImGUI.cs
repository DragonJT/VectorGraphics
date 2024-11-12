namespace VectorGraphics;

using GameEngine;

static class ImGUI {
    static string? id = null;
    static bool dragging = false;
    static float x;
    static float y;
    static float width;
    static float characterScale = 0.2f;
    static float widgetHeight;
    static float lineHeight;
    public static bool mouseOver;

    public static void Start(Rect rect) {
        Graphics.DrawRect(rect, new Color(0.2f,0.2f,0.2f,1));
        Graphics.DrawRectBorder(rect, 2, Color.Blue);
        x = rect.x + 10;
        y = rect.y + 10;
        width = rect.width - 20;
        widgetHeight = Graphics.FontHeight(characterScale) * 1.25f;
        lineHeight = Graphics.FontHeight(characterScale) * 1.5f;
        mouseOver = false;
    }

    public static float Slider(string name, float value){
        var rect = new Rect(x + 150,y,width - 150,widgetHeight);
        if(rect.Contains(Input.MousePosition)) {
            mouseOver = true;
        }
        Graphics.DrawText(new Vector2(x,y), name, characterScale, Color.White);
        y+=lineHeight;
        if(Input.GetButtonDown(Input.MOUSE_BUTTON_1)) {
            if(rect.Contains(Input.MousePosition)) {
                id = name;
                value = Math.Clamp(JMath.InverseLerp(rect.x, rect.x + rect.width, Input.MousePosition.x), 0, 1);
                dragging = true;
            }
        }
        var color = Color.LightCyan;
        if(id == name) {
            if(Input.GetButton(Input.MOUSE_BUTTON_1) && dragging) {
                value = Math.Clamp(JMath.InverseLerp(rect.x, rect.x + rect.width, Input.MousePosition.x), 0, 1);
                Input.Use();
            }
            if(Input.GetButtonUp(Input.MOUSE_BUTTON_1)) {
                dragging = false;
                Input.Use();
            }
            if(Input.GetKey(Input.KEY_LEFT)) {
                value = Math.Clamp(value-Time.DeltaTime,0,1);
                Input.Use();
            }
            if(Input.GetKey(Input.KEY_RIGHT)) {
                value = Math.Clamp(value+Time.DeltaTime,0,1);
                Input.Use();
            }
            color = Color.White;
        }
        Graphics.DrawRectBorder(rect, 2, color);
        var sliderRect = new Rect(rect.x, rect.y, rect.width * value, rect.height);
        Graphics.DrawRect(sliderRect, color);
        return value;
    }

    public static string Options(string current, string[] options) {
        for(var i=0;i<options.Length;i++){
            Color color = Color.Blue;
            Color textColor = Color.White;
            var rect = new Rect(x, y, width, widgetHeight);
            if(rect.Contains(Input.MousePosition)) {
                mouseOver = true;
            }
            if(Input.GetButtonDown(Input.MOUSE_BUTTON_1) && rect.Contains(Input.MousePosition)) {
                current = options[i];
                id = current;
                Input.Use();
            }
            if(options[i] == current) {
                color = Color.White;
                textColor = Color.Black;
            }
            Graphics.DrawRect(rect, color);
            Graphics.DrawText(new Vector2(rect.x + 10, rect.y), options[i], characterScale, textColor);
            Graphics.DrawRectBorder(rect, 2, Color.LightCyan);
            y+=widgetHeight;
        }
        return current;
    }
}