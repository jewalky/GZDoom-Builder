class Pen2
{
    constructor(pos)
    {
        this.angle = Math.PI / 2;
        this.snaptogrid = false;
        this.vertices = [];
        
        if(typeof pos !== 'undefined')
            this.curpos = pos;
        else
            this.curpos = new Vector2D(0, 0);
    }

    moveTo(pos)
    {
        this.curpos = new Vector2D(pos);
    }

    moveForward(distance)
    {
        this.curpos = new Vector2D(
            this.curpos.x + Math.cos(this.angle) * distance,
            this.curpos.y + Math.sin(this.angle) * distance
        );
    }

    turnRightRadians(radians)
    {
        if(typeof radians !== 'undefined')
            this.angle -= radians;
        else
            this.angle -= Math.PI / 2;

        while(this.angle < 0)
            this.angle += Math.PI * 2;        
    }

    turnLeftRadians(radians)
    {
        if(typeof radians !== 'undefined')
            this.angle += radians;
        else
            this.angle += Math.PI / 2;

        while(this.angle > Math.PI * 2)
            this.angle -= Math.PI * 2;   
    }

    turnRight(degrees)
    {
        if(typeof degrees === 'undefined')
            degrees = 90.0;

        this.angle -= degrees * Math.PI / 180.0;

        while(this.angle < 0)
            this.angle += Math.PI * 2;
    }

    turnLeft(degrees)
    {
        if(typeof degrees === 'undefined')
            degrees = 90.0;

        this.angle += degrees * Math.PI / 180.0;

        while(this.angle > Math.PI * 2)
            this.angle -= Math.PI * 2;        
    }

    setAngleRadians(radians)
    {
        this.angle = Math.PI / 2 - radians;        
    }

    setAngle(degrees)
    {
        this.angle = (90 - degrees) * Math.PI / 180.0;
    }

    drawVertex()
    {
        this.vertices.push(this.curpos);
    }

    finishDrawing()
    {
        this.vertices.push(this.vertices[0]);

        let s = '';

        this.vertices.forEach(v => {
            s += v + '\n';
        });

        showMessage(s);

        var result = Map.drawLines(this.vertices);

        this.vertices = [];

        return result;
    }
}