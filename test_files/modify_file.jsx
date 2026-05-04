docRef = app.activeDocument

layerRef = docRef.artLayers[0]

if (layerRef.isBackgroundLayer)
{
    var idDplc = charIDToTypeID( "Dplc" );
    var desc6 = new ActionDescriptor();
    var idnull = charIDToTypeID( "null" );
    var ref1 = new ActionReference();
    var idLyr = charIDToTypeID( "Lyr " );
    var idOrdn = charIDToTypeID( "Ordn" );
    var idTrgt = charIDToTypeID( "Trgt" );
    ref1.putEnumerated( idLyr, idOrdn, idTrgt );
    desc6.putReference( idnull, ref1 );
    var idVrsn = charIDToTypeID( "Vrsn" );
    desc6.putInteger( idVrsn, 5 );
    executeAction( idDplc, desc6, DialogModes.NO );
    layerRef.visible = false
}

// Re-sélectionne le premier artLayer, qu'il est possible de modifier
layerRef = docRef.artLayers[0]
layerRef.rotate(45.0)