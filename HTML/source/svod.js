var mode = 0
var old_t = 0

function shiftSubDiv(n)
{
	var el = document.getElementById('subDiv' + n)
	var im = document.getElementById('image' + n)

	if ( el.style.display == 'none' )
	{
		el.style.display = 'block';
		im.src='../source/minus.png';
	}
	else if ( el.style.display == 'block' )
	{
		el.style.display = 'none';
		im.src='../source/plus.png';
	}
}