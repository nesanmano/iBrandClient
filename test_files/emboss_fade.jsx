// =======================================================
var idDplc = charIDToTypeID( "Dplc" );
    var desc109 = new ActionDescriptor();
    var idnull = charIDToTypeID( "null" );
        var ref7 = new ActionReference();
        var idLyr = charIDToTypeID( "Lyr " );
        var idOrdn = charIDToTypeID( "Ordn" );
        var idTrgt = charIDToTypeID( "Trgt" );
        ref7.putEnumerated( idLyr, idOrdn, idTrgt );
    desc109.putReference( idnull, ref7 );
    var idVrsn = charIDToTypeID( "Vrsn" );
    desc109.putInteger( idVrsn, 5 );
executeAction( idDplc, desc109, DialogModes.NO );

// =======================================================
var idHd = charIDToTypeID( "Hd  " );
    var desc110 = new ActionDescriptor();
    var idnull = charIDToTypeID( "null" );
        var list3 = new ActionList();
            var ref8 = new ActionReference();
            var idLyr = charIDToTypeID( "Lyr " );
            var idBckg = charIDToTypeID( "Bckg" );
            ref8.putProperty( idLyr, idBckg );
        list3.putReference( ref8 );
    desc110.putList( idnull, list3 );
executeAction( idHd, desc110, DialogModes.NO );

// =======================================================
var idEmbs = charIDToTypeID( "Embs" );
    var desc113 = new ActionDescriptor();
    var idAngl = charIDToTypeID( "Angl" );
    desc113.putInteger( idAngl, 135 );
    var idHght = charIDToTypeID( "Hght" );
    desc113.putInteger( idHght, 3 );
    var idAmnt = charIDToTypeID( "Amnt" );
    desc113.putInteger( idAmnt, 100 );
executeAction( idEmbs, desc113, DialogModes.NO );

// =======================================================
var idsetd = charIDToTypeID( "setd" );
    var desc114 = new ActionDescriptor();
    var idnull = charIDToTypeID( "null" );
        var ref9 = new ActionReference();
        var idLyr = charIDToTypeID( "Lyr " );
        var idOrdn = charIDToTypeID( "Ordn" );
        var idTrgt = charIDToTypeID( "Trgt" );
        ref9.putEnumerated( idLyr, idOrdn, idTrgt );
    desc114.putReference( idnull, ref9 );
    var idT = charIDToTypeID( "T   " );
        var desc115 = new ActionDescriptor();
        var idOpct = charIDToTypeID( "Opct" );
        var idPrc = charIDToTypeID( "#Prc" );
        desc115.putUnitDouble( idOpct, idPrc, 75.000000 );
    var idLyr = charIDToTypeID( "Lyr " );
    desc114.putObject( idT, idLyr, desc115 );
executeAction( idsetd, desc114, DialogModes.NO );

