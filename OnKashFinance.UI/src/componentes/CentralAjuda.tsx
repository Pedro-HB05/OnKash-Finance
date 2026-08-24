"use client";

import { useMemo, useState } from "react";
import Link from "next/link";
import { BookOpen, ChevronRight, CircleHelp, ExternalLink, Lightbulb, Search, ShieldCheck } from "lucide-react";
import { useAutenticacao } from "@/contextos/AutenticacaoContexto";

type Guia = { categoria: string; titulo: string; resumo: string; passos: string[]; dica?: string };
type Duvida = { pergunta: string; resposta: string; categoria: string };

const guias: Guia[] = [
  { categoria: "Começando", titulo: "Primeiro acesso", resumo: "Configure a base antes de registrar movimentações.", passos: ["Confirme seu e-mail com o código de seis dígitos.", "Cadastre pelo menos uma conta financeira.", "Cadastre categorias de entrada e saída.", "Registre o primeiro lançamento e confira a Visão geral."], dica: "Contas e categorias ativas são necessárias para criar lançamentos." },
  { categoria: "Dashboard", titulo: "Visão geral e indicadores", resumo: "Entenda rapidamente a situação financeira do período.", passos: ["Use o seletor de período para trocar mês, ano ou intervalo personalizado.", "Saldo mostra quanto existe nas contas ativas.", "Entradas e saídas consideram os lançamentos do período escolhido.", "Os gráficos comparam a evolução e ajudam a identificar mudanças."], dica: "Passe o cursor sobre os gráficos para visualizar os valores de cada ponto." },
  { categoria: "Financeiro", titulo: "Contas financeiras", resumo: "Cadastre bancos, carteiras e outros locais onde o dinheiro fica.", passos: ["Clique em Cadastrar conta.", "Informe nome, tipo e saldo inicial.", "Use os três pontos para editar, desativar, ativar ou excluir.", "Desative contas antigas que possuem histórico; elas deixam de aparecer nos novos lançamentos."], dica: "Excluir remove definitivamente. Desativar preserva o histórico e pode ser revertido." },
  { categoria: "Financeiro", titulo: "Categorias", resumo: "Organize receitas e despesas para melhorar relatórios.", passos: ["Clique em Cadastrar categoria.", "Escolha Entrada para receitas ou Saída para despesas.", "Use nomes claros, como Salário, Alimentação ou Marketing.", "Edite, desative, ative ou exclua pelo menu de três pontos."], dica: "Uma boa categorização torna o DRE e os relatórios mais confiáveis." },
  { categoria: "Financeiro", titulo: "Lançamentos", resumo: "Registre toda entrada, saída ou movimentação financeira.", passos: ["Clique em Novo lançamento.", "Escolha tipo, conta, categoria, data e valor.", "Use Editar e categorizar para corrigir as informações.", "Use Comprovantes para anexar recibos, notas e PDFs.", "Cancelar mantém o registro no histórico, mas retira seu efeito financeiro quando aplicável."], dica: "Use a busca e os filtros para localizar lançamentos rapidamente." },
  { categoria: "Pessoal", titulo: "Cartões e faturas", resumo: "Acompanhe limites, compras, fechamento e pagamento.", passos: ["Cadastre o cartão com limite e dias de fechamento e vencimento.", "Registre compras no cartão e informe o número de parcelas.", "Consulte a fatura para acompanhar o total e o status.", "Ao pagar, selecione a conta usada para a baixa."], dica: "O dia de fechamento define em qual fatura uma compra será incluída." },
  { categoria: "Planejamento", titulo: "Orçamentos mensais", resumo: "Defina quanto pretende gastar em cada categoria.", passos: ["Abra Planejamento e selecione Orçamentos.", "Clique em Novo orçamento.", "Escolha uma categoria de saída, o mês e o limite.", "A barra mostra quanto do orçamento já foi utilizado."], dica: "Os alertas avisam quando o consumo se aproxima ou ultrapassa o limite." },
  { categoria: "Planejamento", titulo: "Lançamentos recorrentes", resumo: "Automatize salários, assinaturas, aluguel e despesas frequentes.", passos: ["Abra a aba Recorrências.", "Clique em Novo recorrente.", "Informe frequência e próxima execução.", "Use Pausar para interromper temporariamente e Ativar para retomar."], dica: "Confira a próxima execução antes de ativar uma recorrência pausada." },
  { categoria: "Inteligência", titulo: "Importar OFX ou CSV", resumo: "Traga o extrato bancário e reduza o trabalho manual.", passos: ["Abra Inteligência e selecione Importar e conciliar.", "Escolha a conta de destino.", "Selecione um arquivo OFX ou CSV.", "Revise a prévia e clique em Confirmar importação.", "Duplicidades identificadas não são importadas novamente."], dica: "Importe sempre para a conta correspondente ao extrato bancário." },
  { categoria: "Inteligência", titulo: "Projeção de caixa e DRE", resumo: "Antecipe saldos e analise o resultado da empresa.", passos: ["Abra a aba Projeção de caixa para consultar entradas e saídas futuras.", "No ambiente empresarial, use DRE simplificada.", "Escolha o período para comparar receitas, despesas, resultado e margem.", "Revise categorias incorretas quando algum valor parecer fora do esperado."], dica: "A qualidade da análise depende das datas e categorias dos lançamentos." },
  { categoria: "Empresarial", titulo: "Contas a pagar e receber", resumo: "Controle vencimentos, baixas e valores pendentes.", passos: ["Cadastre a obrigação ou recebimento com descrição, categoria, valor e vencimento.", "Use busca e filtro de status para localizar registros.", "Clique em Dar baixa quando o pagamento ou recebimento acontecer.", "Use Cancelar somente quando a obrigação deixar de existir."], dica: "Dar baixa registra a data e a conta financeira utilizada." },
  { categoria: "Empresarial", titulo: "Clientes e fornecedores", resumo: "Centralize cadastros usados nas movimentações empresariais.", passos: ["Abra Clientes ou Fornecedores e clique em Cadastrar.", "Informe os dados de contato e documento disponíveis.", "Edite informações pelo menu de três pontos.", "Desative cadastros antigos para preservar o histórico sem usá-los novamente."], dica: "Evite duplicar cadastros com variações do mesmo nome." },
  { categoria: "Empresarial", titulo: "Usuários e permissões", resumo: "Controle quem acessa cada área da empresa.", passos: ["Abra Usuários e cadastre o colaborador.", "Defina o perfil e marque as permissões necessárias.", "Use Editar permissões para alterar acessos.", "Desative o usuário para retirar o acesso sem apagar o histórico."], dica: "Conceda somente os acessos necessários para a função de cada pessoa." },
  { categoria: "Relatórios", titulo: "Relatórios e exportações", resumo: "Filtre os dados e gere arquivos para análise ou compartilhamento.", passos: ["Abra Relatórios.", "Escolha o período e confira os indicadores.", "Clique em Exportar CSV para trabalhar os dados em uma planilha.", "Clique em Exportar PDF para gerar um documento pronto para compartilhar."], dica: "Sem lançamentos no período, os botões de exportação ficam desativados." },
  { categoria: "Conta", titulo: "Perfil, tema e plano", resumo: "Gerencie preferências e acompanhe o uso da conta.", passos: ["Clique no avatar no canto superior direito.", "Meu perfil mostra os dados da conta.", "Configurações permite trocar o tema e consultar preferências.", "Plano e uso mostra o consumo atual e os futuros planos pagos.", "Sair encerra a sessão neste dispositivo."], dica: "Os limites exibidos atualmente são informativos e não bloqueiam o uso." },
  { categoria: "Conta", titulo: "Privacidade e direitos LGPD", resumo: "Acesse, corrija, exporte ou faça solicitações sobre seus dados.", passos: ["Clique no avatar e abra Privacidade e dados.", "Use Baixar meus dados para obter uma cópia em JSON.", "Atualize seu nome na área de correção.", "Para exclusão, anonimização, bloqueio ou outras solicitações, escolha o direito e gere um protocolo.", "Acompanhe o status no histórico da mesma página."], dica: "Leia a Política de Privacidade para conhecer finalidades, compartilhamentos e retenção." },
];

const duvidas: Duvida[] = [
  { categoria: "Acesso", pergunta: "Por que não consigo entrar depois do cadastro?", resposta: "Confirme o e-mail com o código de seis dígitos. Se o código expirou, use Não recebi o código para solicitar outro." },
  { categoria: "Acesso", pergunta: "O que acontece quando minha sessão expira?", resposta: "O sistema encerra a sessão automaticamente e volta para o login. Entre novamente para continuar." },
  { categoria: "Dados", pergunta: "Qual é a diferença entre desativar e excluir?", resposta: "Desativar preserva o registro e permite ativá-lo novamente. Excluir é definitivo e pode ser impedido quando existem movimentações vinculadas." },
  { categoria: "Dados", pergunta: "Por que não consigo excluir uma conta?", resposta: "Contas com lançamentos vinculados precisam ser preservadas para manter o histórico. Nesse caso, use Desativar." },
  { categoria: "Lançamentos", pergunta: "Por que o botão Novo lançamento está desativado?", resposta: "É necessário ter pelo menos uma conta ativa e uma categoria compatível com o tipo do lançamento." },
  { categoria: "Lançamentos", pergunta: "Cancelar um lançamento apaga o histórico?", resposta: "Não. O cancelamento mantém o registro para rastreabilidade, mas sinaliza que ele não deve mais ser considerado como uma movimentação válida." },
  { categoria: "Importação", pergunta: "Posso importar o mesmo extrato duas vezes?", resposta: "O sistema verifica duplicidades. Movimentos já identificados não são criados novamente, mas revise o resultado exibido após a importação." },
  { categoria: "Importação", pergunta: "Quais arquivos posso importar?", resposta: "A área de inteligência aceita extratos OFX e CSV. Para comprovantes, são aceitos PDF, PNG, JPG e WEBP de até 5 MB por arquivo." },
  { categoria: "Relatórios", pergunta: "Por que o relatório não mostra um lançamento?", resposta: "Confira o período, a data, o status e a categoria. Lançamentos cancelados ou fora do intervalo podem não compor os totais." },
  { categoria: "Segurança", pergunta: "Meus dados ficam separados dos outros usuários?", resposta: "Sim. As operações autenticadas utilizam o usuário ou a empresa da sessão para consultar somente os registros correspondentes." },
  { categoria: "Privacidade", pergunta: "Como faço para excluir minha conta ou exportar meus dados?", resposta: "Abra o menu do perfil e acesse Privacidade e dados. A exportação é imediata; pedidos de exclusão geram protocolo e passam por análise das obrigações legais de retenção." },
  { categoria: "Plano", pergunta: "Os limites do plano gratuito já bloqueiam funções?", resposta: "Não. Atualmente eles são informativos. A cobrança e os bloqueios só serão ativados futuramente, com regras comunicadas previamente." },
  { categoria: "Plano", pergunta: "O botão Quero ser avisado faz alguma cobrança?", resposta: "Não. Ele apenas registra seu interesse para receber novidades quando os planos pagos forem lançados." },
];

const botoes = [
  ["Cadastrar / Novo", "Abre o formulário para criar um registro."], ["Salvar / Criar", "Valida e grava as informações preenchidas."],
  ["Três pontos", "Abre as ações disponíveis para aquele registro."], ["Editar", "Abre o registro atual para correção."],
  ["Desativar", "Oculta o registro dos novos usos, preservando o histórico."], ["Ativar", "Torna um registro desativado disponível novamente."],
  ["Excluir", "Remove definitivamente quando não existem vínculos que precisam ser preservados."], ["Cancelar", "Cancela uma movimentação ou obrigação sem apagar sua rastreabilidade."],
  ["Dar baixa", "Confirma que uma conta foi paga ou recebida."], ["Comprovantes", "Abre os anexos do lançamento para enviar, baixar ou excluir arquivos."],
  ["Confirmar importação", "Cria os movimentos válidos mostrados na prévia do extrato."], ["Exportar CSV", "Baixa os dados em formato compatível com planilhas."],
  ["Exportar PDF", "Gera um relatório formatado em PDF."], ["Pausar / Ativar recorrência", "Interrompe ou retoma a geração automática do lançamento."],
];

const normalizar = (texto: string) => texto.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();

export function CentralAjuda() {
  const { sessao } = useAutenticacao();
  const [busca, setBusca] = useState("");
  const [categoria, setCategoria] = useState("Todos");
  const categorias = ["Todos", ...Array.from(new Set(guias.map(g => g.categoria)))];
  const termo = normalizar(busca.trim());
  const guiasVisiveis = useMemo(() => guias.filter(g => (categoria === "Todos" || g.categoria === categoria) && (!termo || normalizar(`${g.titulo} ${g.resumo} ${g.passos.join(" ")} ${g.dica ?? ""}`).includes(termo))), [categoria, termo]);
  const duvidasVisiveis = duvidas.filter(d => !termo || normalizar(`${d.pergunta} ${d.resposta} ${d.categoria}`).includes(termo));
  const inicio = sessao?.tipoConta === "EMPRESARIAL" ? "/empresarial/visao-geral" : "/pessoal/visao-geral";

  return <section className="central-ajuda">
    <header className="hero-ajuda"><div><p className="sobre-titulo">Central de Ajuda</p><h1>Como podemos ajudar?</h1><p>Encontre instruções claras para usar cada área do OnKash Finance.</p><label className="busca-ajuda"><Search size={20}/><input value={busca} onChange={e => setBusca(e.target.value)} placeholder="Busque por lançamento, conta, importação, botão..." aria-label="Buscar na Central de Ajuda"/>{busca && <button onClick={() => setBusca("")} aria-label="Limpar busca">Limpar</button>}</label></div><BookOpen size={76}/></header>

    {!busca && <section className="atalhos-ajuda"><article><span><Lightbulb size={21}/></span><div><strong>Novo por aqui?</strong><p>Comece por conta, categoria e primeiro lançamento.</p></div><Link href={`${inicio}`}>Ir para o início <ChevronRight size={16}/></Link></article><article><span><ShieldCheck size={21}/></span><div><strong>Privacidade e segurança</strong><p>Entenda sessão, permissões e preservação do histórico.</p></div><button onClick={() => { setCategoria("Todos"); setBusca("segurança"); window.scrollTo({ top: 250, behavior: "smooth" }); }}>Ver orientações <ChevronRight size={16}/></button></article></section>}

    <section className="secao-ajuda"><div className="titulo-secao-ajuda"><div><p className="sobre-titulo">Guias de uso</p><h2>Aprenda por módulo</h2></div><span>{guiasVisiveis.length} guia(s)</span></div><div className="filtros-ajuda">{categorias.map(item => <button key={item} className={categoria === item ? "ativo" : ""} onClick={() => setCategoria(item)}>{item}</button>)}</div>{guiasVisiveis.length === 0 ? <div className="estado-vazio"><CircleHelp size={28}/><h3>Nenhum guia encontrado</h3><p>Tente buscar com outra palavra.</p></div> : <div className="grade-guias">{guiasVisiveis.map(guia => <details key={guia.titulo} className="guia-ajuda"><summary><span>{guia.categoria}</span><strong>{guia.titulo}</strong><small>{guia.resumo}</small><ChevronRight size={19}/></summary><div><ol>{guia.passos.map(passo => <li key={passo}>{passo}</li>)}</ol>{guia.dica && <p className="dica-ajuda"><Lightbulb size={16}/><span><strong>Dica:</strong> {guia.dica}</span></p>}</div></details>)}</div>}</section>

    <section className="secao-ajuda"><div className="titulo-secao-ajuda"><div><p className="sobre-titulo">Referência rápida</p><h2>O que cada botão faz</h2></div></div><div className="grade-botoes-ajuda">{botoes.filter(b => !termo || normalizar(b.join(" ")).includes(termo)).map(([nome, explicacao]) => <article key={nome}><strong>{nome}</strong><p>{explicacao}</p></article>)}</div></section>

    <section className="secao-ajuda"><div className="titulo-secao-ajuda"><div><p className="sobre-titulo">FAQ</p><h2>Dúvidas frequentes</h2></div></div><div className="lista-faq">{duvidasVisiveis.map(duvida => <details key={duvida.pergunta}><summary><span>{duvida.categoria}</span><strong>{duvida.pergunta}</strong><ChevronRight size={18}/></summary><p>{duvida.resposta}</p></details>)}</div></section>

    <footer className="rodape-ajuda"><CircleHelp size={24}/><div><strong>Não encontrou sua dúvida?</strong><p>Use a busca acima ou consulte novamente o módulo relacionado.</p></div><Link href={inicio}>Voltar ao sistema <ExternalLink size={16}/></Link></footer>
  </section>;
}
