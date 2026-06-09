Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$assets = Join-Path $root "Assets"
New-Item -ItemType Directory -Path $assets -Force | Out-Null

$pngPath = Join-Path $assets "QuickTxt.png"
$icoPath = Join-Path $assets "QuickTxt.ico"

function New-QuickTxtBitmap {
    param([int]$Size)

    $bitmap = New-Object System.Drawing.Bitmap $Size, $Size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit

    $scale = $Size / 1024.0
    function S([float]$v) { return [float]($v * $scale) }

    $backgroundRect = [System.Drawing.RectangleF]::new((S 56), (S 56), (S 912), (S 912))
    $backgroundPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $radius = S 170
    $diameter = $radius * 2
    $backgroundPath.AddArc($backgroundRect.X, $backgroundRect.Y, $diameter, $diameter, 180, 90)
    $backgroundPath.AddArc($backgroundRect.Right - $diameter, $backgroundRect.Y, $diameter, $diameter, 270, 90)
    $backgroundPath.AddArc($backgroundRect.Right - $diameter, $backgroundRect.Bottom - $diameter, $diameter, $diameter, 0, 90)
    $backgroundPath.AddArc($backgroundRect.X, $backgroundRect.Bottom - $diameter, $diameter, $diameter, 90, 90)
    $backgroundPath.CloseFigure()

    $backgroundBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $backgroundRect, ([System.Drawing.Color]::FromArgb(255, 28, 196, 226)), ([System.Drawing.Color]::FromArgb(255, 17, 85, 228)), 35
    $graphics.FillPath($backgroundBrush, $backgroundPath)

    $shadowBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(70, 0, 0, 0))
    $graphics.FillEllipse($shadowBrush, (S 292), (S 276), (S 478), (S 622))

    $docPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $docPath.AddLine((S 294), (S 210), (S 612), (S 210))
    $docPath.AddLine((S 612), (S 210), (S 790), (S 388))
    $docPath.AddLine((S 790), (S 388), (S 790), (S 804))
    $docPath.AddArc((S 294), (S 748), (S 112), (S 112), 90, 90)
    $docPath.AddLine((S 294), (S 804), (S 294), (S 270))
    $docPath.AddArc((S 294), (S 210), (S 120), (S 120), 180, 90)
    $docPath.CloseFigure()

    $docBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush ([System.Drawing.RectangleF]::new((S 294), (S 210), (S 496), (S 650))), ([System.Drawing.Color]::White), ([System.Drawing.Color]::FromArgb(255, 238, 243, 249)), 90
    $graphics.FillPath($docBrush, $docPath)

    $foldPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $foldPath.AddLine((S 612), (S 210), (S 790), (S 388))
    $foldPath.AddLine((S 790), (S 388), (S 656), (S 388))
    $foldPath.AddArc((S 612), (S 300), (S 88), (S 88), 90, 90)
    $foldPath.CloseFigure()
    $foldBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush ([System.Drawing.RectangleF]::new((S 612), (S 210), (S 178), (S 178))), ([System.Drawing.Color]::FromArgb(255, 255, 255, 255)), ([System.Drawing.Color]::FromArgb(255, 221, 229, 238)), 135
    $graphics.FillPath($foldBrush, $foldPath)

    $linePen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 183, 195, 211)), (S 22)
    $linePen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $linePen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    foreach ($y in @(410, 480, 550)) {
        $graphics.DrawLine($linePen, (S 386), (S $y), (S 662), (S $y))
    }
    $graphics.DrawLine($linePen, (S 386), (S 620), (S 552), (S 620))

    $badgeCenterX = S 692
    $badgeCenterY = S 700
    $badgeRadius = S 160
    $badgeRect = [System.Drawing.RectangleF]::new($badgeCenterX - $badgeRadius, $badgeCenterY - $badgeRadius, $badgeRadius * 2, $badgeRadius * 2)
    $badgeWhite = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 250, 253, 255))
    $graphics.FillEllipse($badgeWhite, $badgeRect)
    $clockPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 20, 185, 180)), (S 34)
    $clockPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $clockPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $graphics.DrawEllipse($clockPen, $badgeRect)
    $graphics.DrawLine($clockPen, $badgeCenterX, $badgeCenterY, $badgeCenterX, (S 614))
    $graphics.DrawLine($clockPen, $badgeCenterX, $badgeCenterY, (S 744), (S 752))

    $boltPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $boltPath.AddPolygon(@(
        [System.Drawing.PointF]::new((S 840), (S 582)),
        [System.Drawing.PointF]::new((S 750), (S 725)),
        [System.Drawing.PointF]::new((S 842), (S 725)),
        [System.Drawing.PointF]::new((S 760), (S 880)),
        [System.Drawing.PointF]::new((S 940), (S 664)),
        [System.Drawing.PointF]::new((S 844), (S 664))
    ))
    $boltOutline = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), (S 26)
    $boltOutline.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $graphics.DrawPath($boltOutline, $boltPath)
    $boltBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush ([System.Drawing.RectangleF]::new((S 740), (S 580), (S 210), (S 300))), ([System.Drawing.Color]::FromArgb(255, 40, 230, 215)), ([System.Drawing.Color]::FromArgb(255, 0, 155, 225)), 45
    $graphics.FillPath($boltBrush, $boltPath)

    $graphics.Dispose()
    return $bitmap
}

function Write-Ico {
    param(
        [string]$Path,
        [int[]]$Sizes
    )

    $pngEntries = @()
    foreach ($size in $Sizes) {
        $bitmap = New-QuickTxtBitmap -Size $size
        $stream = New-Object System.IO.MemoryStream
        $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
        $pngEntries += ,@{
            Size = $size
            Bytes = $stream.ToArray()
        }
        $stream.Dispose()
        $bitmap.Dispose()
    }

    $file = [System.IO.File]::Create($Path)
    $writer = New-Object System.IO.BinaryWriter $file
    $writer.Write([UInt16]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]$pngEntries.Count)

    $offset = 6 + (16 * $pngEntries.Count)
    foreach ($entry in $pngEntries) {
        $dimensionByte = if ($entry.Size -eq 256) { 0 } else { [byte]$entry.Size }
        $writer.Write([byte]$dimensionByte)
        $writer.Write([byte]$dimensionByte)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([UInt16]1)
        $writer.Write([UInt16]32)
        $writer.Write([UInt32]$entry.Bytes.Length)
        $writer.Write([UInt32]$offset)
        $offset += $entry.Bytes.Length
    }

    foreach ($entry in $pngEntries) {
        $writer.Write($entry.Bytes)
    }

    $writer.Dispose()
    $file.Dispose()
}

$preview = New-QuickTxtBitmap -Size 1024
$preview.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
$preview.Dispose()

Write-Ico -Path $icoPath -Sizes @(16, 24, 32, 48, 64, 128, 256)

Write-Host "Created $pngPath"
Write-Host "Created $icoPath"
