"use client";

import { useEffect, useState } from "react";

const chaveTema = "onkash.tema";

export function AlternadorTema() {
  const [temaEscuro, setTemaEscuro] = useState(false);

  useEffect(() => {
    const temaSalvo = localStorage.getItem(chaveTema);
    const escuro = temaSalvo ? temaSalvo === "escuro" : window.matchMedia("(prefers-color-scheme: dark)").matches;
    setTemaEscuro(escuro);
    document.documentElement.dataset.theme = escuro ? "escuro" : "claro";
  }, []);

  const alternar = () => {
    const proximo = !temaEscuro;
    setTemaEscuro(proximo);
    localStorage.setItem(chaveTema, proximo ? "escuro" : "claro");
    document.documentElement.dataset.theme = proximo ? "escuro" : "claro";
  };

  return <button className="alternador-tema" type="button" onClick={alternar} aria-pressed={temaEscuro}>
    <span aria-hidden="true">{temaEscuro ? "☾" : "☀"}</span>
    {temaEscuro ? "Usar modo claro" : "Usar modo escuro"}
  </button>;
}
