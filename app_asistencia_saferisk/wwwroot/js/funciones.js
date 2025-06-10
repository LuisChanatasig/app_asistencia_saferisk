
// ----- ESTADOS GLOBALES -----
let estadoAlmuerzo = "noIniciado";
let estadoBreak = "noIniciado";
let jornadaFinalizada = false;
let tipoEventoSeleccionado = null;
let jornadaIdActual = null;
let tipoJornadaActual = null; // oficina, remoto, mixto
let enTraslado = false;
let trasladoInicioTimestamp = null;
let enPermiso = false; // NUEVO ESTADO


// ---- INICIALIZACIÓN ----
document.addEventListener("DOMContentLoaded", function () {
    actualizarVistaJornada();
    cargarYActualizarTimeline();
    setupEventListeners();
    setupModoSwitch();
});

function setupEventListeners() {
    document.getElementById('formRegistrarLlegada')?.addEventListener('submit', handleRegistrarLlegada);
    document.getElementById('btnBreak')?.addEventListener('click', handleBreak);
    document.getElementById('btnSalida')?.addEventListener('click', handleSalida);
    document.getElementById('btnAlmuerzo')?.addEventListener('click', handleAlmuerzo);
    document.getElementById('btnRegistrarHorasExtra')?.addEventListener('click', handleRegistrarHorasExtra);
    document.getElementById('btnRegistrarExtra')?.addEventListener('click', handleRegistrarExtra);

    // SOLO ESTE listener para abrir el offcanvas y refrescar estado/botones
    document.getElementById('offcanvasExtras')?.addEventListener('show.bs.offcanvas', async function () {
        await cargarYActualizarTimeline(); // <-- esto recalcula el estado real antes de mostrar botones
        tipoEventoSeleccionado = null;
        document.getElementById('btnRegistrarExtra').disabled = true;
        document.getElementById('campoObservacion').style.display = 'none';
        document.getElementById('extraObservaciones').value = '';
        renderAccionesExtra();
    });
}

function setupModoSwitch() {
    const switchModo = document.getElementById('switchModoJornada');
    const btnLlegada = document.getElementById('btnRegistrarLlegada');
    const btnHorasExtra = document.getElementById('btnRegistrarHorasExtra');
    const jornadaRadios = document.getElementById('jornadaRadios');

    if (switchModo && btnLlegada && btnHorasExtra && jornadaRadios) {
        jornadaRadios.style.display = '';
        btnLlegada.style.display = '';
        btnHorasExtra.style.display = 'none';

        switchModo.addEventListener('change', function () {
            if (switchModo.checked) {
                // Esconde radios y botón llegada
                jornadaRadios.style.setProperty('display', 'none', 'important');
                btnLlegada.style.setProperty('display', 'none', 'important');
                btnHorasExtra.style.setProperty('display', '', 'important');
            } else {
                // Muestra radios y botón llegada
                jornadaRadios.style.setProperty('display', '', 'important');
                btnLlegada.style.setProperty('display', '', 'important');
                btnHorasExtra.style.setProperty('display', 'none', 'important');
            }
        });
    }
}



// ===== HANDLERS PRINCIPALES =====
async function handleRegistrarLlegada(e) {
    e.preventDefault();
    const tipoJornada = document.querySelector('input[name="tipoJornada"]:checked').value;
    setBtnLoading('btnRegistrarLlegada', true);

    const datos = await obtenerDatosUbicacion();

    const response = await fetch(window.appRoutes.registrarLlegada, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            tipoJornada, ip: datos.ip, latitud: datos.lat, longitud: datos.lng
        })
    });
    const result = await response.json();
    setBtnLoading('btnRegistrarLlegada', false);
    if (response.ok && result.success) {
        showToast('¡Llegada registrada!', 'success');
        actualizarVistaJornada();
        await cargarYActualizarTimeline();
    } else {
        showToast(result.mensaje || 'No se pudo registrar la llegada.');
    }
}

async function handleBreak() {
    if (jornadaFinalizada) return;
    const btn = this;
    btn.disabled = true;
    setBtnLoading('btnBreak', true); 
    const datos = await obtenerDatosUbicacion();
    if (estadoBreak === "enCurso") {
        await registrarEvento('break_fin', '', datos, function (ok) {
            if (ok) showToast("¡Break finalizado! 👏", "success");
            else btn.disabled = false;
            setBtnLoading('btnBreak', false);  

            cargarYActualizarTimeline();
        });
    } else if (estadoBreak === "noIniciado") {
        await registrarEvento('break_inicio', '', datos, function (ok) {
            if (ok) showToast("¡Disfruta tu break! ☕", "info");
            else btn.disabled = false;
            setBtnLoading('btnBreak', false); 
            cargarYActualizarTimeline();
        });
    }
}

async function handleSalida() {
    if (jornadaFinalizada) return;

    setBtnLoading('btnSalida', true);  // Mostrar loading/spinner

    try {
        const datos = await obtenerDatosUbicacion();

        await registrarEvento('salida', '', datos, async function (ok) {
            if (ok) {
                try {
                    const res = await fetch(window.appRoutes.cerrarJornada, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(jornadaIdActual)
                    });
                    const result = await res.json();

                    if (res.ok && result.success) {
                        showToast("¡Jornada finalizada y cerrada correctamente! 🏁", "primary");
                        bloquearBotonesJornada();
                        await cargarYActualizarTimeline();
                    } else {
                        showToast(result.mensaje || "Error al cerrar la jornada.", "danger");
                    }
                } catch (e) {
                    showToast("No se pudo cerrar la jornada.", "danger");
                }
            }
            setBtnLoading('btnSalida', false); // Oculta el loading al terminar la operación
        });
    } catch (e) {
        showToast("Ocurrió un error inesperado.", "danger");
        setBtnLoading('btnSalida', false); // Asegúrate de ocultar el loading también en el catch principal
    }
}


async function handleAlmuerzo() {
    if (jornadaFinalizada) return;
    const btn = this;
    btn.disabled = true;
    setBtnLoading('btnAlmuerzo', true);  // <--- Activa loading/spinner

    const datos = await obtenerDatosUbicacion();
    if (estadoAlmuerzo === "enCurso") {
        await registrarEvento('almuerzo_fin', '', datos, function (ok) {
            if (ok) showToast("¡Almuerzo finalizado! 👏", "success");
            else btn.disabled = false;
            setBtnLoading('btnAlmuerzo', false); // <--- Quita loading

            cargarYActualizarTimeline();
        });
    } else if (estadoAlmuerzo === "noIniciado") {
        await registrarEvento('almuerzo_inicio', '', datos, function (ok) {
            if (ok) showToast("¡Buen provecho! 🍽️", "info");
            else btn.disabled = false;
            setBtnLoading('btnAlmuerzo', false); // <--- Quita loading

            cargarYActualizarTimeline();
        });
    }
}

async function handleRegistrarHorasExtra() {
    this.disabled = true;
    const datos = await obtenerDatosUbicacion();
    const response = await fetch(window.appRoutes.registrarLlegadaHorasExtra, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            tipoJornada: "horas_extra",
            ip: datos.ip,
            latitud: datos.lat,
            longitud: datos.lng
        })
    });
    const result = await response.json();
    this.disabled = false;
    if (response.ok && result.success) {
        showToast('¡Horas extra registradas!', 'success');
        window.location.href = window.appRoutes.registrarHorasExtra;
    } else {
        showToast(result.mensaje || 'No se pudo registrar las horas extra.');
    }
}

async function handleRegistrarExtra() {
    if (!tipoEventoSeleccionado) {
        showToast('Debes seleccionar un tipo de acción primero.', 'warning');
        return;
    }

    // BLOQUEA todos los botones mientras se registra
    const btnRegistrar = document.getElementById('btnRegistrarExtra');
    btnRegistrar.disabled = true;
    setBtnLoading('btnRegistrarExtra', true);
    // Deshabilita todos los botones de acciones extra
    document.querySelectorAll('#accionesExtraBtns .btn[data-tipo]').forEach(b => b.disabled = true);

    try {
        let obs = document.getElementById('extraObservaciones').value.trim();

        if (["otro", "permiso"].includes(tipoEventoSeleccionado) && !obs) {
            showToast('Debes ingresar una observación.', 'warning');
            document.getElementById('extraObservaciones').focus();
            return;
        }

        if (tipoEventoSeleccionado === "traslado_fin") {
            const destino = await elegirDestinoTraslado();
            if (!destino) {
                showToast('Debes especificar un destino válido.', 'warning');
                return;
            }
            obs += obs ? ` | Destino: ${destino}` : `Destino: ${destino}`;
        }

        const datosUbicacion = await obtenerDatosUbicacion();
        await registrarEvento(tipoEventoSeleccionado, obs, datosUbicacion);

        // Actualización de estado local
        if (tipoEventoSeleccionado === "traslado_inicio") {
            enTraslado = true;
            trasladoInicioTimestamp = new Date();
        } else if (tipoEventoSeleccionado === "traslado_fin") {
            enTraslado = false;
            if (obs.includes("remoto")) tipoJornadaActual = "remoto";
            else tipoJornadaActual = "oficina";
        }

        await cargarYActualizarTimeline();

        // LIMPIA selección visual y variable
        tipoEventoSeleccionado = null;
        document.getElementById('extraObservaciones').value = '';
        document.getElementById('campoObservacion').style.display = 'none';
        document.querySelectorAll('#accionesExtraBtns .btn[data-tipo]').forEach(b => b.classList.remove('active'));

        // Cierra el offcanvas
        const offcanvasEl = bootstrap.Offcanvas.getInstance(document.getElementById('offcanvasExtras'));
        offcanvasEl?.hide();

        showToast('Acción registrada correctamente!', 'success');

    } catch (error) {
        console.error('Error al registrar acción extra:', error);
        showToast('Ocurrió un error al registrar la acción', 'danger');
    } finally {
        setBtnLoading('btnRegistrarExtra', false);
        btnRegistrar.disabled = false;
        // HABILITA los botones de nuevo
        document.querySelectorAll('#accionesExtraBtns .btn[data-tipo]').forEach(b => b.disabled = false);
    }
}


// ===== FUNCIONES DE UI/ESTADO =====

function setBtnLoading(btnId, isLoading) {
    const btn = document.getElementById(btnId);
    if (!btn) return;
    const btnText = document.getElementById('btnText');
    const btnSpinner = document.getElementById('btnSpinner');
    if (isLoading) {
        btn.disabled = true;
        if (btnText) btnText.style.display = "none";
        if (btnSpinner) btnSpinner.style.display = "inline-block";
    } else {
        btn.disabled = false;
        if (btnText) btnText.style.display = "inline";
        if (btnSpinner) btnSpinner.style.display = "none";
    }
}

function actualizarVistaJornada() {
    fetch(window.appRoutes.estadoActual)
        .then(res => res.json())
        .then(data => {
            document.getElementById("estadoJornada").textContent = data.estado ?? "No iniciada";
            if (data.puedeRegistrar) {
                document.getElementById("formLlegadaContainer").style.display = "block";
                document.getElementById("accionesJornada").style.display = "none";
            } else {
                document.getElementById("formLlegadaContainer").style.display = "none";
                document.getElementById("accionesJornada").style.display = "block";
            }
            jornadaIdActual = data.jornadaId ?? null;
            tipoJornadaActual = data.tipoJornada;
        });
}

function cargarYActualizarTimeline() {
    return new Promise((resolve) => {
        const timelineCont = document.getElementById("timelineEventos");
        if (!timelineCont) return resolve();

        timelineCont.innerHTML = `<div class="text-center py-4 text-muted">
        <i class="mdi mdi-progress-clock mdi-36px mb-2"></i>
        <br />Cargando eventos de hoy...
    </div>`;

        fetch(window.appRoutes.timelineHoy)
            .then(res => res.json())
            .then(eventos => {
                timelineCont.innerHTML = "";
                jornadaFinalizada = false;
                estadoAlmuerzo = "noIniciado";
                estadoBreak = "noIniciado";
                enTraslado = false;
                enPermiso = false; // <-- importante

                if (!eventos || !eventos.length) {
                    timelineCont.innerHTML = `<div class="text-center text-muted">Sin eventos registrados aún.</div>`;
                    actualizarBotonAlmuerzo();
                    actualizarBotonBreak();
                    renderAccionesExtra();
                    return resolve();
                }

                let ultimoTraslado = null;
                let ultimoCambioJornada = null;

                eventos.forEach(ev => {
                    timelineCont.innerHTML += `
                                <div class="d-flex align-items-center mb-2">
                                    <span class="me-2">
                                        <i class="mdi ${ev.icono} fs-4"></i>
                                    </span>
                                    <span class="fw-medium">${ev.descripcion}</span>
                                    <span class="ms-auto text-muted small">${ev.hora}</span>
                                    ${ev.obs ? `<span class="ms-2 small text-muted">(${ev.obs})</span>` : ""}
                                </div>`;

                    // Revisa eventos traslado/cambio de modalidad
                    if (ev.codigo === "traslado_inicio" || ev.codigo === "traslado_fin") {
                        ultimoTraslado = ev;
                    }
                    if (ev.codigo === "remoto" || ev.codigo === "oficina" || ev.codigo === "traslado_fin") {
                        ultimoCambioJornada = ev;
                    }

                    // PERMISO DINÁMICO
                    if (ev.codigo === "permiso") enPermiso = true;
                    if (ev.codigo === "permiso_fin") enPermiso = false;

                    if (ev.descripcion && ev.descripcion.toLowerCase().includes("salida")) {
                        jornadaFinalizada = true;
                    }
                });

                if (ultimoTraslado) {
                    enTraslado = (ultimoTraslado.codigo === "traslado_inicio");
                } else {
                    enTraslado = false;
                }

                if (ultimoCambioJornada) {
                    if (ultimoCambioJornada.codigo === "remoto") tipoJornadaActual = "remoto";
                    if (ultimoCambioJornada.codigo === "oficina") tipoJornadaActual = "oficina";
                    if (ultimoCambioJornada.codigo === "traslado_fin") {
                        if (ultimoCambioJornada.obs && ultimoCambioJornada.obs.includes("remoto")) {
                            tipoJornadaActual = "remoto";
                        } else {
                            tipoJornadaActual = "oficina";
                        }
                    }
                }

                actualizarEstadosAlmuerzoBreak(eventos);
                actualizarBotonAlmuerzo();
                actualizarBotonBreak();
                renderAccionesExtra();

                if (jornadaFinalizada) {
                    bloquearBotonesJornada();
                }

                resolve();
            });
    });
}

function actualizarEstadosAlmuerzoBreak(eventos) {
    let lastAlmuerzo = null;
    let lastBreak = null;
    for (let i = eventos.length - 1; i >= 0; i--) {
        if (eventos[i].descripcion === "Inicio de almuerzo" || eventos[i].descripcion === "Fin de almuerzo") {
            if (!lastAlmuerzo) lastAlmuerzo = eventos[i].descripcion;
        }
        if (eventos[i].descripcion === "Inicio de break" || eventos[i].descripcion === "Fin de break") {
            if (!lastBreak) lastBreak = eventos[i].descripcion;
        }
        if (lastAlmuerzo && lastBreak) break;
    }
    if (!lastAlmuerzo) estadoAlmuerzo = "noIniciado";
    else if (lastAlmuerzo === "Inicio de almuerzo") estadoAlmuerzo = "enCurso";
    else if (lastAlmuerzo === "Fin de almuerzo") estadoAlmuerzo = "finalizado";
    if (!lastBreak) estadoBreak = "noIniciado";
    else if (lastBreak === "Inicio de break") estadoBreak = "enCurso";
    else if (lastBreak === "Fin de break") estadoBreak = "finalizado";
}

function bloquearBotonesJornada() {
    const btnAlmuerzo = document.getElementById('btnAlmuerzo');
    const btnBreak = document.getElementById('btnBreak');
    const btnSalida = document.getElementById('btnSalida');
    const btnRegistrarExtra = document.getElementById('btnExtras'); // <-- Nuevo

    [btnAlmuerzo, btnBreak, btnSalida].forEach(btn => {
        if (btn) {
            btn.disabled = true;
            btn.classList.remove("btn-warning", "btn-success", "btn-info", "btn-danger");
            btn.classList.add("btn-secondary");
            btn.innerHTML = `<i class="mdi mdi-lock me-1"></i> Jornada finalizada`;
        }
    });

    // Bloquear el botón de acciones extra
    if (btnRegistrarExtra) {
        btnRegistrarExtra.disabled = true;
        btnRegistrarExtra.classList.remove("btn-primary", "btn-info", "btn-success", "btn-warning", "btn-danger");
        btnRegistrarExtra.classList.add("btn-secondary");
        btnRegistrarExtra.innerHTML = `<i class="mdi mdi-lock me-1"></i> Acciones inhabilitadas`;
    }
}


function actualizarBotonAlmuerzo() {
    const btnAlmuerzo = document.getElementById('btnAlmuerzo');
    const btnBreak = document.getElementById('btnBreak');
    if (!btnAlmuerzo) return;
    if (jornadaFinalizada) {
        btnAlmuerzo.disabled = true;
        btnAlmuerzo.className = "btn btn-secondary";
        btnAlmuerzo.innerHTML = `<i class="mdi mdi-lock me-1"></i> Jornada finalizada`;
        return;
    }
    if (estadoAlmuerzo === "enCurso") {
        btnAlmuerzo.innerHTML = `<i class="mdi mdi-food me-1"></i> Fin de Almuerzo`;
        btnAlmuerzo.className = "btn btn-success";
        btnAlmuerzo.disabled = false;
        if (btnBreak) {
            btnBreak.disabled = true;
            btnBreak.innerHTML = `<i class="mdi mdi-coffee-off-outline me-1"></i> No disponible en almuerzo`;
            btnBreak.className = "btn btn-secondary";
        }
    } else if (estadoAlmuerzo === "noIniciado") {
        btnAlmuerzo.innerHTML = `<i class="mdi mdi-silverware-fork-knife me-1"></i> Almuerzo`;
        btnAlmuerzo.className = "btn btn-warning";
        btnAlmuerzo.disabled = estadoBreak === "enCurso";
        if (btnBreak && estadoBreak === "enCurso") {
            btnAlmuerzo.innerHTML = `<i class="mdi mdi-silverware-fork-knife-off me-1"></i> No disponible en break`;
            btnAlmuerzo.className = "btn btn-secondary";
        }
    } else if (estadoAlmuerzo === "finalizado") {
        btnAlmuerzo.innerHTML = `<i class="mdi mdi-silverware-fork-knife-off me-1"></i> Almuerzo finalizado`;
        btnAlmuerzo.className = "btn btn-secondary";
        btnAlmuerzo.disabled = true;
    }
}

function actualizarBotonBreak() {
    const btnBreak = document.getElementById('btnBreak');
    const btnAlmuerzo = document.getElementById('btnAlmuerzo');
    if (!btnBreak) return;
    if (jornadaFinalizada) {
        btnBreak.disabled = true;
        btnBreak.className = "btn btn-secondary";
        btnBreak.innerHTML = `<i class="mdi mdi-lock me-1"></i> Jornada finalizada`;
        return;
    }
    if (estadoBreak === "enCurso") {
        btnBreak.innerHTML = `<i class="mdi mdi-coffee me-1"></i> Fin de Break`;
        btnBreak.className = "btn btn-success";
        btnBreak.disabled = false;
        if (btnAlmuerzo) {
            btnAlmuerzo.disabled = true;
            btnAlmuerzo.innerHTML = `<i class="mdi mdi-silverware-fork-knife-off me-1"></i> No disponible en break`;
            btnAlmuerzo.className = "btn btn-secondary";
        }
    } else if (estadoBreak === "noIniciado") {
        btnBreak.innerHTML = `<i class="mdi mdi-coffee-outline me-1"></i> Break`;
        btnBreak.className = "btn btn-info";
        btnBreak.disabled = estadoAlmuerzo === "enCurso";
        if (btnAlmuerzo && estadoAlmuerzo === "enCurso") {
            btnBreak.innerHTML = `<i class="mdi mdi-coffee-off-outline me-1"></i> No disponible en almuerzo`;
            btnBreak.className = "btn btn-secondary";
        }
    } else if (estadoBreak === "finalizado") {
        btnBreak.innerHTML = `<i class="mdi mdi-coffee-off-outline me-1"></i> Break finalizado`;
        btnBreak.className = "btn btn-secondary";
        btnBreak.disabled = true;
    }
}

// ---- Acciones extra dinámicas ----
// --- renderAccionesExtra adaptado para "permiso" y "permiso_fin"
function renderAccionesExtra() {
    const cont = document.getElementById('accionesExtraBtns');
    cont.innerHTML = "";

    if (!enTraslado) {
        if (tipoJornadaActual === "oficina") {
            cont.innerHTML += `
                        <button class="btn btn-outline-info w-100 mb-2" data-tipo="traslado_inicio" data-destino="remoto">
                            <i class="mdi mdi-car-arrow-right me-1"></i> Iniciar traslado a remoto
                        </button>
                        <button class="btn btn-outline-primary w-100 mb-2" data-tipo="remoto">
                            <i class="mdi mdi-home-city-outline me-1"></i> Pasar a teletrabajo
                        </button>
                    `;
        }
        if (tipoJornadaActual === "remoto") {
            cont.innerHTML += `
                        <button class="btn btn-outline-info w-100 mb-2" data-tipo="traslado_inicio" data-destino="oficina">
                            <i class="mdi mdi-car-arrow-left me-1"></i> Iniciar traslado a oficina
                        </button>
                        <button class="btn btn-outline-primary w-100 mb-2" data-tipo="oficina">
                            <i class="mdi mdi-office-building-marker-outline me-1"></i> Pasar a oficina
                        </button>
                    `;
        }
    } else {
        cont.innerHTML += `
                    <button class="btn btn-outline-success w-100 mb-2" data-tipo="traslado_fin">
                        <i class="mdi mdi-flag-checkered me-1"></i> Finalizar traslado
                    </button>
                `;
    }

    // ---- PERMISO DINÁMICO ----
    if (!enPermiso) {
        cont.innerHTML += `
                    <button class="btn btn-outline-warning w-100 mb-2" data-tipo="permiso">
                        <i class="mdi mdi-account-cancel me-1"></i> Permiso especial
                    </button>
                `;
    } else {
        cont.innerHTML += `
                    <button class="btn btn-outline-success w-100 mb-2" data-tipo="permiso_fin">
                        <i class="mdi mdi-account-check me-1"></i> Regreso de permiso
                    </button>
                `;
    }

    // Otros botones
    cont.innerHTML += `
    <button class="btn btn-outline-secondary w-100 mb-2" data-tipo="reunion">
        <i class="mdi mdi-account-group me-1"></i> Reunión
    </button>
    <button class="btn btn-outline-secondary w-100 mb-2" data-tipo="soporte técnico visita">
        <i class="mdi mdi-laptop me-1"></i> Soporte técnico visita
    </button>
    <button class="btn btn-outline-dark w-100 mb-2" data-tipo="otro">
        <i class="mdi mdi-dots-horizontal me-1"></i> Otro
    </button>
    `;

    // EVENT LISTENERS MEJORADOS
    document.querySelectorAll('#accionesExtraBtns .btn[data-tipo]').forEach(btn => {
        btn.addEventListener('click', function () {
            // QUITAR active de todos y poner solo al actual
            document.querySelectorAll('#accionesExtraBtns .btn[data-tipo]').forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            tipoEventoSeleccionado = this.getAttribute('data-tipo');

            // Observación solo para ciertos tipos
            if (["otro", "permiso"].includes(tipoEventoSeleccionado)) {
                document.getElementById('campoObservacion').style.display = 'block';
                document.getElementById('extraObservaciones').focus();
            } else {
                document.getElementById('campoObservacion').style.display = 'none';
                document.getElementById('extraObservaciones').value = '';
            }

            document.getElementById('btnRegistrarExtra').disabled = false;
        });
    });
}



// ---- Helpers ----
async function obtenerDatosUbicacion() {
    let ip = "";
    try {
        const res = await fetch("https://api.ipify.org?format=json");
        ip = (await res.json()).ip;
    } catch { ip = ""; }
    function obtenerUbicacion() {
        return new Promise(resolve => {
            if (!navigator.geolocation) return resolve({ lat: null, lng: null });
            navigator.geolocation.getCurrentPosition(
                pos => resolve({ lat: pos.coords.latitude, lng: pos.coords.longitude }),
                () => resolve({ lat: null, lng: null }),
                { enableHighAccuracy: true, timeout: 5000 }
            );
        });
    }
    const ubic = await obtenerUbicacion();
    return { ip, lat: ubic.lat, lng: ubic.lng };
}

async function registrarEvento(tipoEvento, observaciones, datos, callback) {

    const response = await fetch(window.appRoutes.registrarEvento, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            tipoEventoCodigo: tipoEvento,
            observaciones: observaciones,
            ip: datos.ip,
            latitud: datos.lat,
            longitud: datos.lng
        })
    });
    const result = await response.json();
    if (response.ok && result.success) {
        if (typeof callback === "function") callback(true);
    } else {
        if (typeof callback === "function") callback(false);
        showToast(result.mensaje || "No se pudo registrar el evento.");
    }
}

async function elegirDestinoTraslado() {
    let destino = prompt("¿A qué modalidad llegaste? (oficina/remoto)", "oficina");
    return ["oficina", "remoto"].includes(destino) ? destino : "oficina";
}

function showToast(message, type = 'danger') {
    const colorClass = {
        success: 'text-bg-success',
        info: 'text-bg-info',
        warning: 'text-bg-warning',
        primary: 'text-bg-primary',
        danger: 'text-bg-danger'
    }[type] || 'text-bg-danger';
    const toastId = 'toast-' + Date.now();
    const toastHtml = `
    <div id="${toastId}" class="toast align-items-center ${colorClass} border-0 show" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    </div>
    `;
    const container = document.getElementById('toastContainer');
    container.insertAdjacentHTML('beforeend', toastHtml);
    const toastEl = document.getElementById(toastId);
    new bootstrap.Toast(toastEl, { delay: 4000 }).show();
}
