<?php


session_start();

$string = '';

for($i=0;$i<5;$i++)
{
	$string .= chr(rand(97,122));
}

$_SESSION['rand_code'] = $string;


$image = imagecreate(125,60);
$black = imagecolorallocate($image,0,0,0);
$color = imagecolorallocate($image, 255,150,0); //orange
$white = imagecolorallocate($image, 255,255,255);

imagefilledrectangle($image,0,0,125,60,$white);
imagettftext($image,30,0,10,40,$color,$dir.'arial.ttf',$_SESSION['rand_code']);

header('Content-type: image/png');
imagepng($image);



?>
