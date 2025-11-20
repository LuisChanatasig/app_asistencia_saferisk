// ----- ESTADOS GLOBALES -----
let estadoAlmuerzo = "noIniciado";
let estadoBreak = "noIniciado";
let jornadaFinalizada = false;
let tipoEventoSeleccionado = null;
let jornadaIdActual = null;
let tipoJornadaActual = null; // oficina, remoto
let enTraslado = false;
let trasladoInicioTimestamp = null;
let enPermiso = false;
let enSoporteTecnico = false;

// ---- INICIALIZACIÓN ----
document.addEventListener("DOMContentLoaded", function () {
    actualizarVistaJornada();
    cargarYActualizarTimeline();
    setupEventListeners();
    setupModoSwitch();
});

// ---- Helper hora límite (17:30) ----
function esAntesDeHoraLimite(horaLimiteStr = "17:30") {
    const [hStr, mStr] = horaLimiteStr.split(":");
    const limiteHoras = parseInt(hStr, 10);
    const limiteMinutos = parseInt(mStr, 10);

    const ahora = new Date();
    const limite = new Date();

    limite.setHours(limiteHoras, limiteMinutos, 0, 0);

    return ahora < limite; // true si aún no se llega a la hora límite
}

// ---- SETUP ----
function setupEventListeners() {
    document.getElementById('formRegistrarLlegada')?.addEventListener('submit', handleRegistrarLlegada);
    document.getElementById('btnBreak')?.addEventListener('click', handleBreak);
    document.getElementById('btnSalida')?.addEventListener('click', handleSalida);
    document.getElementById('btnAlmuerzo')?.addEventListener('click', handleAlmuerzo);
    document.getElementById('btnRegistrarHorasExtra')?.addEventListener('click', handleRegistrarHorasExtra);
    document.getElementById('btnRegistrarExtra')?.addEventListener('click', handleRegistrarExtra);

    const btnSolicitarAjuste = document.getElementById('btnSolicitarAjuste');
    if (btnSolicitarAjuste) {
        btnSolicitarAjuste.addEventListener('click', handleSolicitarAjuste);
    }

    document.getElementById('offcanvasExtras')?.addEventListener('show.bs.offcanvas', async function () {
        await cargarYActualizarTimeline();
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
        jornadaRadios.classList.remove('d-none');
        btnLlegada.classList.remove('d-none');
        btnHorasExtra.classList.add('d-none');

        switchModo.addEventListener('change', function () {
            if (switchModo.checked) {
                jornadaRadios.classList.add('d-none');
                btnLlegada.classList.add('d-none');
                btnHorasExtra.classList.remove('d-none');
            } else {
                jornadaRadios.classList.remove('d-none');
                btnLlegada.classList.remove('d-none');
                btnHorasExtra.classList.add('d-none');
            }
        });
    }
}

// ===== HANDLERS PRINCIPALES =====
async function handleRegistrarLlegada(e) {
    e.preventDefault();
    if (jornadaFinalizada) return;

    const tipoJornada = document.querySelector('input[name="tipoJornada"]:checked')?.value || "oficina";
    setBtnLoading('btnRegistrarLlegada', 'spinnerLlegada', true, 'btnLlegadaText');

    try {
        const datos = await obtenerDatosUbicacion();

        const response = await fetch(window.appRoutes.registrarLlegada, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                tipoJornada,
                ip: datos.ip,
                latitud: datos.lat,
                longitud: datos.lng
            })
        });

        const result = await response.json();
        if (response.ok && result.success) {
            showToast('¡Llegada registrada!', 'success');
            actualizarVistaJornada();
            await cargarYActualizarTimeline();
        } else {
            showToast(result.mensaje || 'No se pudo registrar la llegada.');
        }
    } catch (err) {
        console.error(err);
        showToast('Ocurrió un error al registrar la llegada.', 'danger');
    } finally {
        setBtnLoading('btnRegistrarLlegada', 'spinnerLlegada', false, 'btnLlegadaText');
    }
}

async function handleBreak() {
    if (jornadaFinalizada) return;

    const btn = document.getElementById('btnBreak');
    if (!btn) return;

    btn.disabled = true;
    setBtnLoading('btnBreak', 'spinnerBreak', true, 'btnBreakText');

    try {
        const datos = await obtenerDatosUbicacion();
        let tipo = "";

        if (estadoBreak === "enCurso") {
            tipo = 'break_fin';
        } else if (estadoBreak === "noIniciado") {
            tipo = 'break_inicio';
        } else {
            return;
        }

        const ok = await registrarEvento(tipo, '', datos);
        if (ok) {
            showToast(
                estadoBreak === "enCurso" ? "¡Break finalizado! 👏" : "¡Disfruta tu break! ☕",
                estadoBreak === "enCurso" ? "success" : "info"
            );
            await cargarYActualizarTimeline();
        }
    } catch (e) {
        console.error(e);
        showToast('Error al registrar break.', 'danger');
    } finally {
        setBtnLoading('btnBreak', 'spinnerBreak', false, 'btnBreakText');
        btn.disabled = false;
    }
}
async function handleSalida() {
    if (jornadaFinalizada) return;

    // 1) Confirmar con SweetAlert si es antes de 17:30
    if (esAntesDeHoraLimite("17:30")) {
        const result = await Swal.fire({
            title: '¿Finalizar jornada?',
            text: 'Estás intentando cerrar tu jornada antes de las 17:30. Si cierras ahora y luego necesitas seguir trabajando, tendrás que solicitar que te la reabran.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, cerrar jornada',
            cancelButtonText: 'No, aún sigo trabajando',
            reverseButtons: true,
            allowOutsideClick: false,
            allowEscapeKey: true
        });

        if (!result.isConfirmed) {
            return; // Usuario canceló → no se cierra la jornada
        }
    }

    // 2) Lógica normal de cierre
    setBtnLoading('btnSalida', 'spinnerSalida', true, 'btnSalidaText');

    try {
        const datos = await obtenerDatosUbicacion();
        const okEvento = await registrarEvento('salida', '', datos);

        if (!okEvento) {
            showToast("No se pudo registrar la salida.", "danger");
            return;
        }

        try {
            const res = await fetch(window.appRoutes.cerrarJornada, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(jornadaIdActual)
            });
            const result = await res.json();

            if (res.ok && result.success) {
                await Swal.fire({
                    title: 'Jornada finalizada',
                    text: 'Tu jornada se cerró correctamente.',
                    icon: 'success',
                    confirmButtonText: 'Aceptar'
                });

                bloquearBotonesJornada();
                await cargarYActualizarTimeline();
            } else {
                showToast(result.mensaje || "Error al cerrar la jornada.", "danger");
            }
        } catch (e) {
            console.error(e);
            showToast("No se pudo cerrar la jornada.", "danger");
        }
    } catch (e) {
        console.error(e);
        showToast("Ocurrió un error inesperado.", "danger");
    } finally {
        setBtnLoading('btnSalida', 'spinnerSalida', false, 'btnSalidaText');
    }
}


async function handleAlmuerzo() {
    if (jornadaFinalizada) return;

    const btn = document.getElementById('btnAlmuerzo');
    if (!btn) return;

    btn.disabled = true;
    setBtnLoading('btnAlmuerzo', 'spinnerAlmuerzo', true, 'btnAlmuerzoText');

    try {
        const datos = await obtenerDatosUbicacion();
        let tipo = "";

        if (estadoAlmuerzo === "enCurso") {
            tipo = 'almuerzo_fin';
        } else if (estadoAlmuerzo === "noIniciado") {
            tipo = 'almuerzo_inicio';
        } else {
            return;
        }

        const ok = await registrarEvento(tipo, '', datos);
        if (ok) {
            showToast(
                estadoAlmuerzo === "enCurso" ? "¡Almuerzo finalizado! 👏" : "¡Buen provecho! 🍽️",
                estadoAlmuerzo === "enCurso" ? "success" : "info"
            );
            await cargarYActualizarTimeline();
        }
    } catch (e) {
        console.error(e);
        showToast("Error al registrar almuerzo.", "danger");
    } finally {
        setBtnLoading('btnAlmuerzo', 'spinnerAlmuerzo', false, 'btnAlmuerzoText');
        btn.disabled = false;
    }
}

async function handleRegistrarHorasExtra() {
    const btn = document.getElementById('btnRegistrarHorasExtra');
    if (!btn) return;

    btn.disabled = true;
    setBtnLoading('btnRegistrarHorasExtra', 'spinnerHorasExtra', true, 'btnHorasExtraText');

    try {
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

        if (response.ok && result.success) {
            showToast('¡Horas extra registradas!', 'success');
            window.location.href = window.appRoutes.registrarHorasExtra;
        } else {
            showToast(result.mensaje || 'No se pudo registrar las horas extra.');
        }
    } catch (e) {
        console.error(e);
        showToast("Error al registrar horas extra.", "danger");
    } finally {
        setBtnLoading('btnRegistrarHorasExtra', 'spinnerHorasExtra', false, 'btnHorasExtraText');
        btn.disabled = false;
    }
}

async function handleRegistrarExtra() {
    if (!tipoEventoSeleccionado) {
        showToast('Debes seleccionar un tipo de acción primero.', 'warning');
        return;
    }

    const btnRegistrar = document.getElementById('btnRegistrarExtra');
    if (!btnRegistrar) return;

    btnRegistrar.disabled = true;
    setBtnLoading('btnRegistrarExtra', 'spinnerRegistrarExtra', true, 'btnRegistrarExtraText');
    document.querySelectorAll('#accionesExtraBtns .btn[data-tipo]').forEach(b => b.disabled = true);

    try {
        let obs = document.getElementById('extraObservaciones').value.trim();

        if (["otro", "permiso", "soporte_inspeccion_inicio"].includes(tipoEventoSeleccionado) && !obs) {
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
        const ok = await registrarEvento(tipoEventoSeleccionado, obs, datosUbicacion);
        if (!ok) return;

        if (tipoEventoSeleccionado === "traslado_inicio") {
            enTraslado = true;
            trasladoInicioTimestamp = new Date();
        } else if (tipoEventoSeleccionado === "traslado_fin") {
            enTraslado = false;
        } else if (tipoEventoSeleccionado === "permiso") {
            enPermiso = true;
        } else if (tipoEventoSeleccionado === "permiso_fin") {
            enPermiso = false;
        } else if (tipoEventoSeleccionado === "soporte_inspeccion_inicio") {
            enSoporteTecnico = true;
        } else if (tipoEventoSeleccionado === "soporte_inspeccion_fin") {
            enSoporteTecnico = false;
        }

        await cargarYActualizarTimeline();

        tipoEventoSeleccionado = null;
        const obsInput = document.getElementById('extraObservaciones');
        const campoObs = document.getElementById('campoObservacion');
        if (obsInput) obsInput.value = '';
        if (campoObs) campoObs.classList.add('d-none');

        document.querySelectorAll('#accionesExtraBtns .btn[data-tipo]').forEach(b => b.classList.remove('active'));

        const offcanvasEl = bootstrap.Offcanvas.getInstance(document.getElementById('offcanvasExtras'));
        offcanvasEl?.hide();

        showToast('Acción registrada correctamente!', 'success');
    } catch (error) {
        console.error('Error al registrar acción extra:', error);
        showToast('Ocurrió un error al registrar la acción', 'danger');
    } finally {
        setBtnLoading('btnRegistrarExtra', 'spinnerRegistrarExtra', false, 'btnRegistrarExtraText');
        btnRegistrar.disabled = false;
        document.querySelectorAll('#accionesExtraBtns .btn[data-tipo]').forEach(b => b.disabled = false);
    }
}

async function handleSolicitarAjuste() {
    if (!jornadaIdActual) {
        await Swal.fire({
            icon: 'info',
            title: 'Sin jornada actual',
            text: 'No se ha detectado una jornada abierta o asociada al día de hoy. Igual puedes describir el problema en la solicitud.',
            confirmButtonText: 'Continuar'
        });
    }

    const { value: formValues } = await Swal.fire({
        title: 'Solicitar ajuste de jornada',
        html: `
            <div class="mb-3 text-start">
                <label class="form-label">Tipo de ajuste</label>
                <select id="swal-tipo-ajuste" class="form-select">
                    <option value="">Seleccione...</option>
                    <option value="REAPERTURA">Reapertura de jornada</option>
                    <option value="CORRECCION_ENTRADA">Corrección de hora de entrada</option>
                    <option value="CORRECCION_SALIDA">Corrección de hora de salida</option>
                    <option value="MARCACION_OLVIDADA">Olvidé marcar (entrada/salida)</option>
                    <option value="OTRO">Otro</option>
                </select>
            </div>
            <div class="mb-3 text-start">
                <label class="form-label">Descripción</label>
                <textarea id="swal-descripcion-ajuste" class="form-control" rows="3"
                          placeholder="Describe brevemente el problema (ej: olvidé marcar la salida ayer a las 17:10)..."></textarea>
            </div>
        `,
        focusConfirm: false,
        showCancelButton: true,
        confirmButtonText: 'Enviar solicitud',
        cancelButtonText: 'Cancelar',
        preConfirm: () => {
            const tipo = document.getElementById('swal-tipo-ajuste').value;
            const desc = document.getElementById('swal-descripcion-ajuste').value.trim();

            if (!tipo) {
                Swal.showValidationMessage('Selecciona un tipo de ajuste.');
                return false;
            }
            if (!desc) {
                Swal.showValidationMessage('Ingresa una breve descripción.');
                return false;
            }

            return { tipoAjuste: tipo, descripcion: desc };
        }
    });

    if (!formValues) {
        return; // cancelado
    }

    try {
        const response = await fetch(window.appRoutes.crearSolicitudAjuste, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                jornadaId: jornadaIdActual,
                tipoAjuste: formValues.tipoAjuste,
                descripcion: formValues.descripcion
            })
        });

        const result = await response.json();

        if (response.ok && result.success) {
            showToast(result.mensaje || 'Solicitud registrada correctamente.', 'success');
            await Swal.fire({
                icon: 'success',
                title: 'Solicitud enviada',
                text: 'Tu solicitud de ajuste ha sido registrada. Soporte la atenderá en breve.',
                confirmButtonText: 'Aceptar'
            });
        } else {
            showToast(result.mensaje || 'No se pudo registrar la solicitud.', 'danger');
        }
    } catch (e) {
        console.error('Error al crear solicitud de ajuste:', e);
        showToast('Ocurrió un error al registrar la solicitud.', 'danger');
    }
}


// ===== FUNCIONES DE UI/ESTADO =====
function setBtnLoading(btnId, spinnerId, isLoading, textId) {
    const btn = document.getElementById(btnId);
    const spinner = document.getElementById(spinnerId);
    const textSpan = textId ? document.getElementById(textId) : null;
    if (!btn || !spinner) return;

    if (isLoading) {
        btn.disabled = true;
        spinner.classList.remove('d-none');
        if (textSpan) textSpan.classList.add('d-none');
    } else {
        btn.disabled = false;
        spinner.classList.add('d-none');
        if (textSpan) textSpan.classList.remove('d-none');
    }
}

function actualizarVistaJornada() {
    fetch(window.appRoutes.estadoActual)
        .then(res => res.json())
        .then(data => {
            const lblEstado = document.getElementById("estadoJornada");
            const frm = document.getElementById("formLlegadaContainer");
            const acciones = document.getElementById("accionesJornada");

            if (lblEstado) lblEstado.textContent = data.estado ?? "No iniciada";

            if (data.puedeRegistrar) {
                frm?.classList.remove('d-none');
                acciones?.classList.add('d-none');
            } else {
                frm?.classList.add('d-none');
                acciones?.classList.remove('d-none');
            }

            jornadaIdActual = data.jornadaId ?? null;
            tipoJornadaActual = data.tipoJornada;
        })
        .catch(err => {
            console.error("Error en estadoActual:", err);
        });
}

function cargarYActualizarTimeline() {
    return new Promise((resolve) => {
        const timelineCont = document.getElementById("timelineEventos");
        if (!timelineCont) return resolve();

        timelineCont.innerHTML = `
            <div class="text-center py-4 text-muted">
                <i class="mdi mdi-progress-clock mdi-36px mb-2"></i>
                <br />Cargando eventos de hoy...
            </div>`;

        fetch(window.appRoutes.timelineHoy)
            .then(res => {
                if (!res.ok) throw new Error("HTTP " + res.status);
                return res.json();
            })
            .then(eventos => {
                timelineCont.innerHTML = "";
                jornadaFinalizada = false;
                estadoAlmuerzo = "noIniciado";
                estadoBreak = "noIniciado";
                enTraslado = false;
                enPermiso = false;
                enSoporteTecnico = false;

                if (!eventos || !eventos.length) {
                    timelineCont.innerHTML = `<div class="text-center text-muted">Sin eventos registrados aún.</div>`;
                    actualizarBotonAlmuerzo();
                    actualizarBotonBreak();
                    renderAccionesExtra();
                    return resolve();
                }

                let ultimoTraslado = null;
                let ultimoCambioJornada = null;
                let ultimoPermiso = null;
                let ultimoSoporteTecnico = null;

                eventos.forEach(ev => {
                    let mapHtml = '';
                    if (ev.latitud && ev.longitud) {
                        const mapId = 'map-' + Math.random().toString(36).substr(2, 9);
                        mapHtml = `
                            <button class="btn btn-sm btn-outline-primary py-0 px-2 mb-1"
                                    onclick="document.getElementById('${mapId}').style.display='block'; this.style.display='none';">
                                <i class="mdi mdi-map-marker-outline"></i> Ver mapa
                            </button>
                            <div id="${mapId}" style="display:none;">
                                <iframe
                                    width="180"
                                    height="120"
                                    class="rounded-3 shadow-sm"
                                    style="border:0; margin-top:2px;"
                                    loading="lazy"
                                    allowfullscreen
                                    src="https://maps.google.com/maps?q=${ev.latitud},${ev.longitud}&z=16&output=embed">
                                </iframe>
                                <br>
                                <button class="btn btn-sm btn-outline-secondary py-0 px-2 mt-1"
                                        onclick="this.parentElement.style.display='none'; this.parentElement.previousElementSibling.style.display='inline-block';">
                                    <i class="mdi mdi-close"></i> Cerrar mapa
                                </button>
                            </div>`;
                    }

                    timelineCont.innerHTML += `
                        <div class="mb-3">
                            <div class="d-flex align-items-center">
                                <span class="me-2">
                                    <i class="mdi ${ev.icono} fs-4"></i>
                                </span>
                                <span class="fw-medium">${ev.descripcion}</span>
                                <span class="ms-auto text-muted small">${ev.hora}</span>
                                ${ev.obs ? `<span class="ms-2 small text-muted">(${ev.obs})</span>` : ""}
                            </div>
                            <div class="ms-5 small text-muted">
                                IP: ${ev.ip || 'N/D'}
                                ${ev.latitud && ev.longitud ? `<span class="mx-2">| Coordenadas: ${Number(ev.latitud).toFixed(5)}, ${Number(ev.longitud).toFixed(5)}</span>` : ""}
                                <br>
                                ${mapHtml}
                            </div>
                        </div>`;

                    if (["traslado_inicio", "traslado_fin"].includes(ev.codigo)) {
                        ultimoTraslado = ev;
                    }
                    if (["remoto", "oficina", "traslado_fin"].includes(ev.codigo)) {
                        ultimoCambioJornada = ev;
                    }
                    if (["permiso", "permiso_fin"].includes(ev.codigo)) {
                        ultimoPermiso = ev;
                    }
                    if (["soporte_inspeccion_inicio", "soporte_inspeccion_fin"].includes(ev.codigo)) {
                        ultimoSoporteTecnico = ev;
                    }

                    if (ev.descripcion && ev.descripcion.toLowerCase().includes("salida")) {
                        jornadaFinalizada = true;
                    }
                });

                if (ultimoTraslado) {
                    enTraslado = (ultimoTraslado.codigo === "traslado_inicio");
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

                if (ultimoPermiso) {
                    enPermiso = (ultimoPermiso.codigo === "permiso");
                }

                if (ultimoSoporteTecnico) {
                    enSoporteTecnico = (ultimoSoporteTecnico.codigo === "soporte_inspeccion_inicio");
                }

                actualizarEstadosAlmuerzoBreak(eventos);
                actualizarBotonAlmuerzo();
                actualizarBotonBreak();
                renderAccionesExtra();

                if (jornadaFinalizada) {
                    bloquearBotonesJornada();
                }

                resolve();
            }).catch(err => {
                timelineCont.innerHTML = `<div class="text-danger text-center">Error al cargar el timeline.<br>${err}</div>`;
                console.error("Error en timelineHoy:", err);
                resolve();
            });
    });
}

function actualizarEstadosAlmuerzoBreak(eventos) {
    let lastAlmuerzo = null;
    let lastBreak = null;
    for (let i = eventos.length - 1; i >= 0; i--) {
        if (["Inicio de almuerzo", "Fin de almuerzo"].includes(eventos[i].descripcion)) {
            if (!lastAlmuerzo) lastAlmuerzo = eventos[i].descripcion;
        }
        if (["Inicio de break", "Fin de break"].includes(eventos[i].descripcion)) {
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
    const btnExtras = document.getElementById('btnExtras');

    [btnAlmuerzo, btnBreak, btnSalida].forEach(btn => {
        if (btn) {
            btn.disabled = true;
            btn.className = "btn btn-secondary btn-lg";
            btn.innerHTML = `<i class="mdi mdi-lock me-1"></i> Jornada finalizada`;
        }
    });

    if (btnExtras) {
        btnExtras.disabled = true;
        btnExtras.className = "btn btn-secondary btn-lg";
        btnExtras.innerHTML = `<i class="mdi mdi-lock me-1"></i> Acciones inhabilitadas`;
    }
}

function actualizarBotonAlmuerzo() {
    const btnAlmuerzo = document.getElementById('btnAlmuerzo');
    const btnBreak = document.getElementById('btnBreak');
    if (!btnAlmuerzo) return;

    if (jornadaFinalizada) {
        btnAlmuerzo.disabled = true;
        btnAlmuerzo.className = "btn btn-secondary btn-lg";
        btnAlmuerzo.innerHTML = `<i class="mdi mdi-lock me-1"></i> Jornada finalizada`;
        return;
    }

    if (estadoAlmuerzo === "enCurso") {
        btnAlmuerzo.innerHTML = `<span id="btnAlmuerzoText"><i class="mdi mdi-food me-1"></i> Fin de Almuerzo</span><span id="spinnerAlmuerzo" class="spinner-border spinner-border-sm ms-2 d-none" role="status" aria-hidden="true"></span>`;
        btnAlmuerzo.className = "btn btn-success btn-lg";
        btnAlmuerzo.disabled = false;
        if (btnBreak) {
            btnBreak.disabled = true;
            btnBreak.className = "btn btn-secondary btn-lg";
            btnBreak.innerHTML = `<i class="mdi mdi-coffee-off-outline me-1"></i> No disponible en almuerzo`;
        }
    } else if (estadoAlmuerzo === "noIniciado") {
        btnAlmuerzo.innerHTML = `<span id="btnAlmuerzoText"><i class="mdi mdi-silverware-fork-knife me-1"></i> Almuerzo</span><span id="spinnerAlmuerzo" class="spinner-border spinner-border-sm ms-2 d-none" role="status" aria-hidden="true"></span>`;
        btnAlmuerzo.className = "btn btn-warning btn-lg";
        btnAlmuerzo.disabled = (estadoBreak === "enCurso");
        if (btnBreak && estadoBreak === "enCurso") {
            btnAlmuerzo.innerHTML = `<i class="mdi mdi-silverware-fork-knife-off me-1"></i> No disponible en break`;
            btnAlmuerzo.className = "btn btn-secondary btn-lg";
        }
    } else if (estadoAlmuerzo === "finalizado") {
        btnAlmuerzo.innerHTML = `<i class="mdi mdi-silverware-fork-knife-off me-1"></i> Almuerzo finalizado`;
        btnAlmuerzo.className = "btn btn-secondary btn-lg";
        btnAlmuerzo.disabled = true;
    }
}

function actualizarBotonBreak() {
    const btnBreak = document.getElementById('btnBreak');
    const btnAlmuerzo = document.getElementById('btnAlmuerzo');
    if (!btnBreak) return;

    if (jornadaFinalizada) {
        btnBreak.disabled = true;
        btnBreak.className = "btn btn-secondary btn-lg";
        btnBreak.innerHTML = `<i class="mdi mdi-lock me-1"></i> Jornada finalizada`;
        return;
    }

    if (estadoBreak === "enCurso") {
        btnBreak.innerHTML = `<span id="btnBreakText"><i class="mdi mdi-coffee me-1"></i> Fin de Break</span><span id="spinnerBreak" class="spinner-border spinner-border-sm ms-2 d-none" role="status" aria-hidden="true"></span>`;
        btnBreak.className = "btn btn-success btn-lg";
        btnBreak.disabled = false;
        if (btnAlmuerzo) {
            btnAlmuerzo.disabled = true;
            btnAlmuerzo.className = "btn btn-secondary btn-lg";
            btnAlmuerzo.innerHTML = `<i class="mdi mdi-silverware-fork-knife-off me-1"></i> No disponible en break`;
        }
    } else if (estadoBreak === "noIniciado") {
        btnBreak.innerHTML = `<span id="btnBreakText"><i class="mdi mdi-coffee-outline me-1"></i> Break</span><span id="spinnerBreak" class="spinner-border spinner-border-sm ms-2 d-none" role="status" aria-hidden="true"></span>`;
        btnBreak.className = "btn btn-info btn-lg";
        btnBreak.disabled = (estadoAlmuerzo === "enCurso");
        if (btnAlmuerzo && estadoAlmuerzo === "enCurso") {
            btnBreak.innerHTML = `<i class="mdi mdi-coffee-off-outline me-1"></i> No disponible en almuerzo`;
            btnBreak.className = "btn btn-secondary btn-lg";
        }
    } else if (estadoBreak === "finalizado") {
        btnBreak.innerHTML = `<i class="mdi mdi-coffee-off-outline me-1"></i> Break finalizado`;
        btnBreak.className = "btn btn-secondary btn-lg";
        btnBreak.disabled = true;
    }
}

// ---- Acciones extra dinámicas ----
function renderAccionesExtra() {
    const cont = document.getElementById('accionesExtraBtns');
    if (!cont) return;
    cont.innerHTML = "";

    if (!enTraslado) {
        if (tipoJornadaActual === "oficina") {
            cont.innerHTML += `
                <button class="btn btn-outline-info w-100" data-tipo="traslado_inicio" data-destino="remoto">
                    <i class="mdi mdi-car-arrow-right me-1"></i> Iniciar traslado a remoto
                </button>
                <button class="btn btn-outline-primary w-100" data-tipo="remoto">
                    <i class="mdi mdi-home-city-outline me-1"></i> Pasar a teletrabajo
                </button>`;
        }
        if (tipoJornadaActual === "remoto") {
            cont.innerHTML += `
                <button class="btn btn-outline-info w-100" data-tipo="traslado_inicio" data-destino="oficina">
                    <i class="mdi mdi-car-arrow-left me-1"></i> Iniciar traslado a oficina
                </button>
                <button class="btn btn-outline-primary w-100" data-tipo="oficina">
                    <i class="mdi mdi-office-building-marker-outline me-1"></i> Pasar a oficina
                </button>`;
        }
    } else {
        cont.innerHTML += `
            <button class="btn btn-outline-success w-100" data-tipo="traslado_fin">
                <i class="mdi mdi-flag-checkered me-1"></i> Finalizar traslado
            </button>`;
    }

    if (!enPermiso) {
        cont.innerHTML += `
            <button class="btn btn-outline-warning w-100" data-tipo="permiso">
                <i class="mdi mdi-account-cancel me-1"></i> Permiso especial
            </button>`;
    } else {
        cont.innerHTML += `
            <button class="btn btn-outline-success w-100" data-tipo="permiso_fin">
                <i class="mdi mdi-account-check me-1"></i> Regreso de permiso
            </button>`;
    }

    if (!enSoporteTecnico) {
        cont.innerHTML += `
            <button class="btn btn-outline-secondary w-100" data-tipo="soporte_inspeccion_inicio">
                <i class="mdi mdi-laptop me-1"></i> Iniciar soporte técnico o inspección
            </button>`;
    } else {
        cont.innerHTML += `
            <button class="btn btn-outline-success w-100" data-tipo="soporte_inspeccion_fin">
                <i class="mdi mdi-laptop-check me-1"></i> Finalizar soporte técnico o inspección
            </button>`;
    }

    cont.innerHTML += `
        <button class="btn btn-outline-secondary w-100" data-tipo="reunion">
            <i class="mdi mdi-account-group me-1"></i> Reunión
        </button>
        <button class="btn btn-outline-dark w-100" data-tipo="otro">
            <i class="mdi mdi-dots-horizontal me-1"></i> Otro
        </button>`;

    document.querySelectorAll('#accionesExtraBtns .btn[data-tipo]').forEach(btn => {
        btn.addEventListener('click', function () {
            document.querySelectorAll('#accionesExtraBtns .btn[data-tipo]').forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            tipoEventoSeleccionado = this.getAttribute('data-tipo');

            const campoObs = document.getElementById('campoObservacion');
            const obsInput = document.getElementById('extraObservaciones');

            if (["otro", "permiso", "soporte_inspeccion_inicio"].includes(tipoEventoSeleccionado)) {
                campoObs?.classList.remove('d-none');
                obsInput?.focus();
            } else {
                campoObs?.classList.add('d-none');
                if (obsInput) obsInput.value = '';
            }

            const btnReg = document.getElementById('btnRegistrarExtra');
            if (btnReg) btnReg.disabled = false;
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
                () => {
                    alert("No se pudo obtener la ubicación. Se usará una ubicación de ejemplo.");
                    resolve({ lat: -2.170998, lng: -79.922359 }); // Guayaquil
                },
                { enableHighAccuracy: true, timeout: 5000 }
            );
        });
    }

    const ubic = await obtenerUbicacion();
    console.log("Datos de ubicación obtenidos:", { ip, lat: ubic.lat, lng: ubic.lng });

    return { ip, lat: ubic.lat, lng: ubic.lng };
}

async function registrarEvento(tipoEvento, observaciones, datos) {
    try {
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
            return true;
        } else {
            showToast(result.mensaje || "No se pudo registrar el evento.");
            return false;
        }
    } catch (e) {
        console.error(e);
        showToast("Error al registrar el evento.", "danger");
        return false;
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
        <div id="${toastId}" class="toast align-items-center ${colorClass} border-0 show"
             role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto"
                        data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>`;

    const container = document.getElementById('toastContainer');
    container.insertAdjacentHTML('beforeend', toastHtml);
    const toastEl = document.getElementById(toastId);
    new bootstrap.Toast(toastEl, { delay: 4000 }).show();
}
