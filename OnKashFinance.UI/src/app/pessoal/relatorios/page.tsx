import { AreaAutenticada } from "@/componentes/AreaAutenticada";
import { RelatorioFinanceiro } from "@/componentes/RelatorioFinanceiro";
export default () => (
  <AreaAutenticada tipo="pessoal">
    <RelatorioFinanceiro tipo="pessoal" />
  </AreaAutenticada>
);
