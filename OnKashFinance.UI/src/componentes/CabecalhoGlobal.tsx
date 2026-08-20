"use client";
import { MenuPerfil } from "@/componentes/MenuPerfil";

export function CabecalhoGlobal() {
  return (
    <header className="topbar-global">
      <span>OnKash Finance</span>
      <MenuPerfil />
    </header>
  );
}
