

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

function Color(r,g,b){
    return `rgb(${r*255}, ${g*255}, ${b*255})`
}

function Vector2(x,y){
    return {x:x, y:y};
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
editor.mode = undefined;
editor.selectable = new SelectableButtons(sideMenuDiv, (name)=>editor.mode = name);
editor.fill = true;
editor.objs = [];
editor.modes = {};

editor.color = ColorInput(sideMenuDiv, '#77ff00', ChangeColor);
FileExtension();
FreeLineExtension();
LineExtension();
SelectExtension();
editor.selectable.SelectFirst();
editor.menu.Create(menuDiv);

function CallEvent(name, args){
    var mode = editor.modes[editor.mode];
    if(mode[name])
        mode[name](...args);
}

function KeyDown(e){
    if(e.target != ctx.canvas)
        return;
    CallEvent('KeyDown', [e]);
}

function MouseUp(e){
    if(e.target != ctx.canvas)
        return;
    var rect = e.target.getBoundingClientRect();
    var x = e.clientX - rect.left;
    var y = e.clientY - rect.top;
    CallEvent('MouseUp', [{x:x, y:y}]);
}

function MouseMove(e){
    if(e.target != ctx.canvas)
        return;
    var rect = e.target.getBoundingClientRect();
    var x = e.clientX - rect.left;
    var y = e.clientY - rect.top;
    CallEvent('MouseMove', [{x:x, y:y}]);
}

function MouseDown(e){
    if(e.target != ctx.canvas)
        return;
    var rect = e.target.getBoundingClientRect();
    var x = e.clientX - rect.left;
    var y = e.clientY - rect.top;
    CallEvent('MouseDown', [{x:x, y:y}]);
}

function ChangeColor(value){
    console.log(value);
    CallEvent('ChangeColor', [value]);
}

function Update(){
    ctx.fillStyle = 'black';
    ctx.fillRect(0,0,ctx.canvas.width,ctx.canvas.height);
    for(var o of editor.objs){
        editor.modes[o.type].Draw(o);
    }
    CallEvent('DrawEditor', []);
    requestAnimationFrame(Update);
}

addEventListener('mouseup', MouseUp);
addEventListener('mousemove', MouseMove);
addEventListener('mousedown', MouseDown);
addEventListener('keydown', KeyDown);
Update();