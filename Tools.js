
function FreeLineTool(e){
    if(e.type == 'Awake'){
        editor.selectable.AddButton('FreeLine');
    }
    if(editor.tool == 'FreeLine'){
        if(e.type == 'mousedown'){
            var pos = CanvasPos(e);
            if(pos){
                editor.current = [Vector2(pos.x, pos.y)];
            }
        }
        else if(e.type == 'mousemove'){
            var pos = CanvasPos(e);
            if(pos){
                if(editor.current)
                    editor.current.push(Vector2(pos.x, pos.y));
            }
        }
        else if(e.type == 'mouseup'){
            var pos = CanvasPos(e);
            if(pos){
                if(editor.current){
                    editor.objs.push({type:'Line', color:editor.color.value, points:editor.current});
                    editor.current = undefined;
                }
            }
        }
        else if(e.type == 'Update'){
            if(editor.current)
                DrawLine(editor.current, editor.color.value);
        }
    }
}


function LineTool(e){
    if(e.type == 'Awake'){
        editor.selectable.AddButton('Line');
    }
    if(editor.tool == 'Line'){
        if(e.type == 'mousedown'){
            var pos = CanvasPos(e);
            if(pos){
                if(!editor.current)
                    editor.current = [Vector2(pos.x, pos.y)];
                else
                    editor.current.push(Vector2(pos.x, pos.y));
            }
        }
        else if(e.type == 'keydown'){
            if(e.key == 'Enter'){
                editor.objs.push({type:'Line', color:editor.color.value, points:editor.current});
                editor.current = undefined;
            }
        }
        else if(e.type == 'Update'){
            if(editor.current)
                DrawLine(editor.current, editor.color.value);
        }
    }
}


function SelectTool(e){
    function MoveToBack(){
        for(var s of editor.selected)
            DeleteObj(s);
        editor.objs.unshift(...editor.selected);
    }

    function GetPathClickedOn(x,y){
        for(var i=editor.objs.length-1;i>=0;i--){
            var o = editor.objs[i];
            if(PointInsideLine(o.points, x, y))
                return o;
        }
    }

    function DeleteObj(obj){
        RemoveFromArray(editor.objs, obj);
    }

    if(e.type == 'Awake'){
        editor.selectable.AddButton('Select');
        editor.menu.Add('Edit', 'MoveToBack', MoveToBack);
    }
    if(editor.tool == 'Select'){
        if(e.type == 'mousedown'){
            var pos = CanvasPos(e);
            if(pos){
                var obj = GetPathClickedOn(pos.x, pos.y);
                if(obj)
                    editor.selected = [obj];
                else
                    editor.selected = [];
            }
        }
        else if(e.type == 'Update'){
            for(var s of editor.selected){
                ctx.strokeStyle = 'red';
                ctx.setLineDash([10, 10]);
                ctx.lineWidth = 5;
                ctx.stroke(GetPath(s.points));
            }
        }
        else if(e.type == 'keydown'){
            if(e.key == 'Backspace'){
                for(var s of editor.selected)
                    DeleteObj(s);
                editor.selected = [];
            }
        }
        else if(e.type == 'ChangeColor'){
            for(var s of editor.selected){
                s.color = e.color;
            }
        }
    }
}

function ToolEvent(e){
    FreeLineTool(e);
    LineTool(e);
    SelectTool(e);
}