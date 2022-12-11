function Color(r,g,b){
    return `rgb(${r*255}, ${g*255}, ${b*255})`
}

function Vector2(x,y){
    return {x:x, y:y};
}

function CanvasPos(e){
    if(e.target != ctx.canvas)
        return;
    var rect = e.target.getBoundingClientRect();
    var x = e.clientX - rect.left;
    var y = e.clientY - rect.top;
    return Vector2(x,y);
}

function Distance(v1, v2){
    var dx = v2.x - v1.x;
    var dy = v2.y - v1.y;
    return Math.sqrt(dx*dx , dy*dy);
}

function GetPath(points){
    var path = new Path2D();
    path.moveTo(points[0].x, points[0].y);
    for(var i=1;i<points.length;i++){
        path.lineTo(points[i].x, points[i].y);
    }
    return path;
}

function DrawLine(points, color){
    var path = GetPath(points);
    ctx.fillStyle = color;
    ctx.fill(path);
}

function PointInsideLine(points, x, y){
    var path = GetPath(points);
    return ctx.isPointInPath(path, x, y);
}

function RemoveFromArray(array, item){
    var element = array.indexOf(item);
    if(element>=0)
        array.splice(element, 1);
}

function RGBToHex(r,g,b){
    function componentToHex(c) {
        var hex = Math.floor(c*255).toString(16);
        return hex.length == 1 ? "0" + hex : hex;
      }
      
    return "#" + componentToHex(r) + componentToHex(g) + componentToHex(b);
}

function LoadFile(func) {
    readFile = function(e) {
        var file = e.target.files[0];
        if (!file) {
            return;
        }
        var reader = new FileReader();
        reader.onload = function(e) {
            var contents = e.target.result;
            fileInput.func(contents)
            document.body.removeChild(fileInput)
        }
        reader.readAsText(file)
    }
    var fileInput = document.createElement("input")
    fileInput.type='file'
    fileInput.style.display='none'
    fileInput.onchange=readFile
    fileInput.func=func
    document.body.appendChild(fileInput)
    fileInput.click();
}

function SaveFile(name, data){
    const file = new File([data], name, {
        type: 'text/plain',
    });
        
    const link = document.createElement('a');
    const url = URL.createObjectURL(file);
    
    link.href = url;
    link.download = file.name;
    document.body.appendChild(link);
    link.click();
    
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
}

function Save(){
    SaveFile('save.jt', JSON.stringify(editor.objs));
}

function Load(){
    LoadFile(c=>editor.objs = JSON.parse(c));
}

var mainDiv = GridDiv(document.body, '100px auto', '25px auto');
var cornerDiv = Div(mainDiv);
var menuDiv = Div(mainDiv);
var sideMenuDiv = Div(mainDiv);
var ctxDiv = Div(mainDiv);
var ctx = CreateContext(ctxDiv, c=>{
    c.width = window.innerWidth - 100;
    c.height = window.innerHeight - 25;
});
document.body.style.overflow = 'hidden';

var editor = {};
editor.menu = new Menu();
editor.selectable = new SelectableButtons(sideMenuDiv, (name)=>editor.tool = name);
editor.objs = [];
editor.selected = [];

function CallEvent(e){
    if(e.type == 'Awake'){
        editor.color = ColorInput(sideMenuDiv,  RGBToHex(0,1,0.5), ChangeColor);
        editor.menu.Add('File', 'Save', Save);
        editor.menu.Add('File', 'Load', Load);
    }
    if(e.type == 'Update'){
        ctx.fillStyle = Color(0.3,0.3,0.3);
        ctx.fillRect(0,0,ctx.canvas.width,ctx.canvas.height);
        for(var o of editor.objs){
            DrawLine(o.points, o.color);
        }
    }
    ToolEvent(e);
    if(e.type == 'Awake'){
        editor.menu.Create(menuDiv);
        editor.selectable.SelectFirst();
    }
}

function ChangeColor(value){
    CallEvent({type:'ChangeColor', color:value});
}

function Update(){
    CallEvent({type:'Update'});
    requestAnimationFrame(Update);
}

function Awake(){
    CallEvent({type:'Awake'});
}

addEventListener('mouseup', CallEvent);
addEventListener('mousemove', CallEvent);
addEventListener('mousedown', CallEvent);
addEventListener('keydown', CallEvent);
Awake();
Update();