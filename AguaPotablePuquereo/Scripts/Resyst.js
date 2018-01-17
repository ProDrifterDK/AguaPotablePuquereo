var Resyst = {};



Resyst.Toast = function (data, tabla) {
    if (data.mensaje) {
        if (data.exito) {
            Resyst.Exito(data.mensaje);
        } else {
            Resyst.Error(data.mensaje);
        }

        if (tabla) {
            Resyst.Reload(tabla);
        }
    }
}

Resyst.Error = function (mensaje) {
    Materialize.toast(mensaje, 3000, 'red');
}

Resyst.Exito = function (mensaje) {
    Materialize.toast(mensaje, 3000, 'green');
}

Resyst.ToastRecargar = function (data, tabla) {
    if (data.mensaje) {
        if (data.exito) {
            Materialize.toast(data.mensaje, 3000, 'green');
        } else {
            Materialize.toast(data.mensaje, 3000, 'red');
        }
    }

    if (tabla) {
        Resyst.Reload(tabla);
    }
}

Resyst.Reload = function (tabla) {
    $("#" + tabla).DataTable().ajax.reload();
}

Resyst.ValidarRut = function (rut) {

    // Valida el rut con su cadena completa "XXXXXXXX-X"
    if (!/^[0-9]+[-|‐]{1}[0-9kK]{1}$/.test(rut))
        return false;
    var tmp = rut.split('-');
    var digv = tmp[1];
    var rut = tmp[0];
    if (digv == 'K') digv = 'k';
    return (Resyst.dv(rut) == digv);
}

Resyst.dv = function(T) {
    var M = 0, S = 1;
    for (; T; T = Math.floor(T / 10))
        S = (S + T % 10 * (9 - M++ % 6)) % 11;
    return S ? S - 1 : 'k';
}

