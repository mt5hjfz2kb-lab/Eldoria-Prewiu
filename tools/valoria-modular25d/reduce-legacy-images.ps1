param([Parameter(Mandatory=$true)][string]$ImageFolder)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
foreach($file in Get-ChildItem -LiteralPath $ImageFolder -Filter '*.input'){
    $image=[Drawing.Image]::FromFile($file.FullName)
    try{
        $factor=[Math]::Min(1.0,256.0/[Math]::Max($image.Width,$image.Height))
        $width=[Math]::Max(1,[int][Math]::Round($image.Width*$factor))
        $height=[Math]::Max(1,[int][Math]::Round($image.Height*$factor))
        $bitmap=[Drawing.Bitmap]::new($width,$height)
        try{
            $graphics=[Drawing.Graphics]::FromImage($bitmap)
            try{
                $graphics.CompositingMode=[Drawing.Drawing2D.CompositingMode]::SourceCopy
                $graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.DrawImage($image,0,0,$width,$height)
            }finally{$graphics.Dispose()}
            $bitmap.Save($file.FullName+'.png',[Drawing.Imaging.ImageFormat]::Png)
        }finally{$bitmap.Dispose()}
    }finally{$image.Dispose()}
}
