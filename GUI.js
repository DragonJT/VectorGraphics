function Div(parent){
    var div = document.createElement('div');
    parent.appendChild(div);
    return div;
}

function GridDiv(parent, columns, rows){
    var div = Div(parent);
    div.style.display = 'grid';
    div.style.gridTemplateColumns = columns;
    div.style.gridTemplateRows = rows;
    return div;
}

function CreateContext(parent, setSize){
    var canvas = document.createElement('canvas');
    canvas.tabIndex = '0';
    parent.appendChild(canvas);
    parent.style.margin = '0px';
    setSize(canvas);
    addEventListener('resize', e=>setSize(canvas));
    return canvas.getContext('2d');
}

function Button(parent, innerHTML, onclick){
    var button = document.createElement('button');
    parent.appendChild(button);
    button.innerHTML = innerHTML;
    button.onclick = onclick;
    return button;
}

function DivButton(parent, innerHTML, onclick){
    var div = Div(parent);
    var button = Button(div, innerHTML, onclick);
    button.style.width = '100%';
    button.style.margin = '0px';
    button.style.height = '25px';
    return button;
}

function Input(parent, type, value){
    var input = document.createElement('input');
    parent.appendChild(input);
    input.type = type;
    input.value = value;
    return input;
}

function ColorInput(parent, color, onChangeColor){
    var div = Div(parent);
    var input = Input(div, 'color', color);
    input.oninput = ()=>onChangeColor(input.value);
    input.style.width = '100%';
    input.style.height = '25px';
    return input;
}

class SelectableButtons{
    constructor(parent, onSelect){
        this.onSelect = onSelect;
        this.buttons = new Map();
        this.div = Div(parent);
    }

    Select(name){
        this.name = name;
        for(var b of this.buttons.values()){
            b.style.backgroundColor = Color(1,1,1);
        }
        var button = this.buttons.get(name);
        button.style.backgroundColor = Color(0,0.5,1);
        this.onSelect(name);
    }

    AddButton(name){
        this.buttons.set(name, DivButton(this.div, name, ()=>this.Select(name)));
    }

    SelectFirst(){
        const [firstKey] = this.buttons.keys();
        if(firstKey)
            this.Select(firstKey);
    }
}

function DropDown(position, items){
    function OnClick(item){
        return ()=>{
            item.onclick();
            document.body.removeChild(menu);
        };
    }

    var menu = Div(document.body);
    menu.style.position = 'absolute';
    menu.style.left = position.x+'px';
    menu.style.top = position.y+'px';
    menu.style.width = '150px';
    menu.style.height = items.length*25+'px';

    for(var i of items){
        DivButton(menu, i.name, OnClick(i));
    }
}

class Menu{
    constructor(){
        this.items = new Map();
    }

    Add(menu, name, onclick){
        if(!this.items.has(menu)){
            this.items.set(menu, []);
        }
        this.items.get(menu).push({name:name, onclick:onclick});
    }

    static Click(button, items){
        return ()=>{
            var rect = button.getBoundingClientRect();
            DropDown(Vector2(rect.left, rect.top+25), items);
        };
    }

    Create(parent){
        for(var i of this.items.keys()){
            var button = Button(parent, i);
            button.onclick = Menu.Click(button, this.items.get(i));
            button.style.width = '100px';
        }
    }
}