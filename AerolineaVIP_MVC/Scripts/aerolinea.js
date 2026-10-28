// CÓDIGO PROPIO ADAPTADO - Javascript de interfaz de Mela.
var $ = function (i) { return document.getElementById(i) };
if (!document.getElementById('go')) { /* Solo activo en Home/Index */ } else {

/* Pestañas Ida y vuelta / Solo ida */
var tabs = document.querySelectorAll('.tabs button');
tabs.forEach(function (b) {
    b.onclick = function () {
        tabs.forEach(function (x) { x.classList.remove('on') });
        b.classList.add('on');
        $('rf').style.opacity = b.dataset.t === 'ow' ? .4 : 1;
        $('r').disabled = b.dataset.t === 'ow';
    }
});

/* Aeropuertos */
/* Aeropuertos VIP del Proyecto */
var AP = 'Quito|Ecuador|Aeropuerto Mariscal Sucre|UIO;Guayaquil|Ecuador|Aeropuerto José Joaquín de Olmedo|GYE;Manta|Ecuador|Aeropuerto Eloy Alfaro|MEC;Galápagos|Ecuador|Aeropuerto Ecológico de Seymour|GPS'
    .split(';').map(function (s) { var a = s.split('|'); return { c: a[0], p: a[1], n: a[2], k: a[3] } })
    .sort(function (a, b) { return a.c.localeCompare(b.c) });

function norm(s) { return s.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '') }

function setupAP(inp, dd) {
    function draw() {
        var q = norm(inp.value.replace(/\(.*\)/, '').trim());
        var l = AP.filter(function (a) { return !q || norm(a.c + ' ' + a.p + ' ' + a.n + ' ' + a.k).indexOf(q) > -1 });
        dd.innerHTML = '<h5>Todos los aeropuertos</h5>' + (l.length ? l.map(function (a) {
            return '<div class="ap" tabindex="0" data-i="' + AP.indexOf(a) + '"><span>' + a.c + ', ' + a.p + '<small>' + a.n + '</small></span><span>' + a.k + '</span></div>';
        }).join('') : '<p style="font-size:14px;padding:10px 0">No encontramos aeropuertos con esa búsqueda.</p>');
        dd.hidden = false;
    }
    function pick(e) {
        var r = e.target.closest('.ap'); if (!r) return;
        var a = AP[r.dataset.i];
        inp.value = a.c + ' (' + a.k + ')';
        dd.hidden = true;
        if (inp.id === 'o' && !$('d').value) $('d').focus();
    }
    inp.addEventListener('focus', function () { inp.select(); draw() });
    inp.addEventListener('input', draw);
    dd.addEventListener('mousedown', function (e) { e.preventDefault() });
    dd.addEventListener('click', pick);
    dd.addEventListener('keydown', function (e) { if (e.key === 'Enter') pick(e) });
}
setupAP($('o'), $('ddo'));
setupAP($('d'), $('ddd'));

$('sw').onclick = function () { var v = $('o').value; $('o').value = $('d').value; $('d').value = v };

/* Pasajeros */
var P = { a: 1, j: 0, n: 0, b: 0 };
var PL = [['a', 'Adultos', 'Desde 15 años'], ['j', 'Jóvenes', 'De 12 a 14 años'], ['n', 'Niños', 'De 2 a 11 años'], ['b', 'Bebés', 'Menores de 2 años (1 por adulto)']];
function tot() { return P.a + P.j + P.n + P.b }
function drawP() {
    $('pl').innerHTML = PL.map(function (r) {
        var k = r[0], min = k === 'a' ? 1 : 0, full = tot() >= 9 || (k === 'b' && P.b >= P.a);
        return '<div class="pr"><div>' + r[1] + '<small>' + r[2] + '</small></div><div class="ct"><button data-k="' + k + '" data-d="-1" ' + (P[k] <= min ? 'disabled' : '') + ' aria-label="Menos ' + r[1] + '">−</button><span>' + P[k] + '</span><button data-k="' + k + '" data-d="1" ' + (full ? 'disabled' : '') + ' aria-label="Más ' + r[1] + '">+</button></div></div>';
    }).join('');
    $('pb').textContent = tot() + (tot() > 1 ? ' pasajeros' : '') + ' ▾';
}
$('pl').addEventListener('click', function (e) {
    var b = e.target.closest('button'); if (!b) return;
    var k = b.dataset.k; P[k] += +b.dataset.d;
    if (k === 'a' && P.b > P.a) P.b = P.a;
    drawP();
});
$('pb').onclick = function () { var h = $('pop').hidden; $('pop').hidden = !h; $('pb').setAttribute('aria-expanded', h) };
$('pok').onclick = function () { $('pop').hidden = true };
$('infx').onclick = function () { $('inf').hidden = true };
$('pol').onclick = function (e) { e.preventDefault(); $('m').textContent = 'Los jóvenes de 12 a 14 años viajan acompañados de un adulto.' };
drawP();

/* Cerrar paneles al hacer clic fuera o con Escape */
document.addEventListener('click', function (e) {
    if (!e.target.closest('.f')) ['ddo', 'ddd', 'pop'].forEach(function (i) { $(i).hidden = true });
});
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') { ['ddo', 'ddd', 'pop'].forEach(function (i) { $(i).hidden = true }); $('lov').hidden = true }
});

/* Fechas y búsqueda */
var today = new Date().toISOString().slice(0, 10);
$('s').min = today; $('r').min = today; $('r').disabled = true; $('rf').style.opacity = .4;
$('s').onchange = function () {
    $('r').min = $('s').value || today;
    if ($('r').value && $('r').value < $('s').value) $('r').value = '';
};
$('go').onclick = function () {
    if (document.querySelector('.tabs .on').dataset.t !== 'ow') {
        $('m').textContent = 'Por el momento, la compra confirma un solo trayecto (solo ida).'; return;
    }
    // Integración Mela -> Nico: invocar la búsqueda real del backend.
    var o = $('o').value, d = $('d').value, s = $('s').value;
    var oa = /\(([A-Z]{3})\)/.exec(o), da = /\(([A-Z]{3})\)/.exec(d);
    if (!oa || !da || !s) { $('m').textContent = 'Selecciona aeropuertos válidos y fecha de salida.'; return; }
    if (oa[1] === da[1]) { $('m').textContent = 'El origen y destino deben ser distintos.'; return; }
    if (tot() !== 1) { $('m').textContent = 'En esta primera entrega, confirma una plaza por reserva. Selecciona 1 pasajero.'; return; }
    window.location.href = '/Vuelos/Index?fecha=' + encodeURIComponent(s)
      + '&origenCodigo=' + encodeURIComponent(oa[1])
      + '&destinoCodigo=' + encodeURIComponent(da[1]);
};

/* Ubicación e idioma */
var C = [['🇨🇴', 'Colombia', 'COP'], ['🇦🇷', 'Argentina', 'ARS'], ['🇧🇴', 'Bolivia', 'USD'], ['🇧🇷', 'Brasil', 'BRL'], ['🇨🇱', 'Chile', 'CLP'], ['🇨🇷', 'Costa Rica', 'USD'], ['🇪🇨', 'Ecuador', 'USD'], ['🇪🇸', 'España', 'EUR'], ['🇺🇸', 'Estados Unidos', 'USD'], ['🇲🇽', 'México', 'MXN'], ['🇵🇦', 'Panamá', 'USD'], ['🇵🇪', 'Perú', 'PEN']];
$('cs').innerHTML = C.map(function (c, i) { return '<option value="' + i + '">' + c[0] + ' ' + c[1] + ' (' + c[2] + ')</option>' }).join('');
$('locb').onclick = function () { $('lov').hidden = false; $('cs').focus() };
$('lx').onclick = function () { $('lov').hidden = true };
$('lov').onclick = function (e) { if (e.target === $('lov')) $('lov').hidden = true };
$('lok').onclick = function () {
    var c = C[$('cs').value];
    $('locb').textContent = c[0] + ' ' + c[2] + ' ▾';
    document.documentElement.lang = { 'Español': 'es', 'English': 'en', 'Português': 'pt' }[$('ls').value];
    $('lov').hidden = true;
    $('m').textContent = 'Ubicación: ' + c[1] + ' · Moneda: ' + c[2] + ' · Idioma: ' + $('ls').value;
};

}
