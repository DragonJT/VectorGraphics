
function FreeLineExtension(){
    var current = undefined;
    editor.selectable.AddButton('freeLine');

    editor.modes.freeLine = {
        MouseDown: (pos)=>{
            current = [Vector2(pos.x, pos.y)];
        },
        MouseMove: (pos)=>{
            if(current)
                current.push(Vector2(pos.x, pos.y));
        },
        MouseUp: (pos)=>{
            if(current){
                editor.objs.push({type:'freeLine', color:editor.color.value, points:current});
                current = undefined;
            }
        },
        Draw:(o)=>{
            DrawLine(o.points, o.color);
        },
        DrawEditor:()=>{
            if(current)
                DrawLine(current, editor.color.value);
        }
    };
}


function LineExtension(){
    var current = undefined;
    editor.selectable.AddButton('line');
    
    editor.modes.line = {
        MouseDown: (pos)=>{
            if(!current)
                current = [Vector2(pos.x, pos.y)];
            else
                current.push(Vector2(pos.x, pos.y));
        },
        KeyDown: (e)=>{
            if(e.key == 'Enter'){
                editor.objs.push({type:'line', color:editor.color.value, points:current});
                current = undefined;
            }
        },
        Draw:(o)=>{
            DrawLine(o.points, o.color);
        },
        DrawEditor:()=>{
            if(current)
                DrawLine(current, editor.color.value);
        }
    };
}


function SelectExtension(){
    var selected = [];
    editor.selectable.AddButton('select');
    editor.menu.Add('Edit', 'MoveToBack', MoveToBack);

    function MoveToBack(){
        for(var s of selected)
            DeleteObj(s);
        editor.objs.unshift(...selected);
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

    editor.modes.select = {
        MouseDown:(pos)=>{
            var obj = GetPathClickedOn(pos.x, pos.y);
            if(obj)
                selected = [obj];
            else
                selected = [];
        },
        DrawEditor:()=>{
            for(var s of selected){
                ctx.strokeStyle = 'blue';
                ctx.lineWidth = 5;
                ctx.stroke(GetPath(s.points));
            }
        },
        KeyDown:e=>{
            if(e.key == 'Backspace'){
                for(var s of selected)
                    DeleteObj(s);
                selected = [];
            }
        },
        ChangeColor:(color)=>{
            for(var s of selected){
                s.color = color;
            }
        }
    };
}

function FileExtension(){
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
    /*function Save(){
        require('fs').writeFileSync('save.jt', JSON.stringify(editor.objs));
    }

    function Load(){
        editor.objs = JSON.parse(require('fs').readFileSync('save.jt'));
    }*/

    editor.menu.Add('File', 'Save', Save);
    editor.menu.Add('File', 'Load', Load);
}