var Resyst = {};

Resyst.Toast = function (data, tabla) {
    if (data.mensaje) {
        if (data.exito) {
            Materialize.toast(data.mensaje, 3000, 'green');
        } else {
            Materialize.toast(data.mensaje, 3000, 'red');
        }

        if (tabla) {
            Resyst.Reload(tabla);
        }
    }
}

Resyst.Reload = function (tabla) {
    $("#" + tabla).DataTable().ajax.reload();
}