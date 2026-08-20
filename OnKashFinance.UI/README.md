# OnKash Finance UI

Frontend em Next.js para o OnKash Finance.

## Executar

```bash
npm install
npm run dev
```

Abra `http://localhost:3000`.

O arquivo `.env.local` já está preparado para encaminhar as requisições para a API publicada. Para usar uma API local, altere `API_URL` para `http://localhost:5202` e reinicie o Next.

## Publicação no Render

Na API, configure a variável de ambiente abaixo para aceitar qualquer endereço público do frontend, além dos endereços locais já configurados:

```text
Cors__AllowedOrigins=*
```

Se preferir limitar o acesso, informe os endereços separados por `;`, por exemplo:

```text
Cors__AllowedOrigins=http://localhost:3000;http://localhost:5173;https://seu-frontend.onrender.com
```

## Verificação

```bash
npm run lint
npm run build
```
