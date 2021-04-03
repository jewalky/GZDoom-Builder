function Pen(pos) {
	this.angle = Math.PI / 2;
	this.snaptogrid = false;
	//this.ListOfDrawnVertex = System.Collections.Generic.List(UDB.Geometry.DrawnVertex);
	//this.vertices = new this.ListOfDrawnVertex();
	this.vertices = [];
	
	if(typeof pos !== 'undefined')
		this.curpos = pos;
	else
		this.curpos = new Vector2D(0, 0);
}

Pen.prototype.drawVertex = function() {
	this.vertices.push(this.curpos);
}

Pen.prototype.finishDrawing = function() {
	this.vertices.push(this.vertices[0]);
	
	var result = Map.drawLines(this.vertices);
	
	//this.vertices = new this.ListOfDrawnVertex();
	this.vertices = [];
	
	return result;
}

Pen.prototype.moveForward = function(distance) {
	this.curpos = new Vector2D(
		this.curpos.x + Math.cos(this.angle) * distance,
		this.curpos.y + Math.sin(this.angle) * distance
	);
}

Pen.prototype.moveTo = function(pos) {
	this.curpos = new Vector2D(pos.x, pos.y);
}

Pen.prototype.turnRight = function(radians) {
	if(typeof radians !== 'undefined')
		this.angle -= radians;
	else
		this.angle -= Math.PI / 2;
	
	while(this.angle < 0)
		this.angle += Math.PI * 2;
}

Pen.prototype.turnLeft = function(radians) {
	if(typeof radians !== 'undefined')
		this.angle += radians;
	else
		this.angle += Math.PI / 2;
	
	while(this.angle > Math.PI * 2)
		this.angle -= Math.PI * 2;
}

Pen.prototype.turnRightDegrees = function(degrees) {
	this.angle -= degrees * Math.PI / 180.0;
	
	while(this.angle < 0)
		this.angle += Math.PI * 2;
}

Pen.prototype.turnLeftDegrees = function(degrees) {
	this.angle += degrees * Math.PI / 180.0;
	
	while(this.angle > Math.PI * 2)
		this.angle -= Math.PI * 2;
}

Pen.prototype.setAngle = function(radians) {
	this.angle = Math.PI / 2 - radians;
}

Pen.prototype.setAngleDegrees = function(degrees) {
	this.angle = (90 - degrees) * Math.PI / 180.0;
}