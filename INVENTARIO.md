# Inventário trimestral — roteiro

## Fluxo no sistema
1. **Inventários → Novo inventário**: escolha ano/trimestre (padrão: último trimestre fechado). Todos os tanques ativos de todos os postos ativos entram no inventário.
2. **Baixar formulário** (`Inventario.CriarFormulario`): gera `Inventario_<ano>_T<n>.xlsx`.
3. Preencher a coluna amarela **MEDIÇÃO (cm)** (uma linha por tanque).
4. **Importar medições** (`Inventario.FazerInventario`): converte cm → litros com `TanqueService.CalcularVolume` e grava uma `Medicao` no histórico de cada tanque. Se houver qualquer erro, nada é gravado.
5. **Informar PPL**: um campo de preço por grupo (mesmo combustível + mesmo fornecedor) replica o valor para todos os tanques do grupo (`Inventario.AlterarPpl`); exceções podem ser digitadas tanque a tanque.
6. **Planilha de estoque** (`Inventario.CriarPlanilhaSaida`): gera `INV_dd-MM-aaaa.xlsx` no formato "ESTOQUE dd/mm/aaaa" — um bloco por posto (QTDADE | PREÇO | TOTAL) em duas colunas, tanques do mesmo combustível e preço somados, subtotal por posto e total geral, com fórmulas. Disponível para qualquer inventário já existente. O nome do grupo no cabeçalho vem de `Inventario:NomeGrupo` no `appsettings.json`.

## Layout da planilha (aba "Leituras")
| Coluna | Conteúdo | Preencher? |
|---|---|---|
| A — ID TANQUE | ID do tanque instalado no posto (PostoTanque), chave da importação | **não alterar** |
| B — POSTO | nome do posto | não |
| C — TANQUE | identificação no posto, ou T1, T2… (ordem de instalação) | não |
| D — COMBUSTÍVEL / E — FORNECEDOR | identificação | não |
| F — CAPACIDADE (m³) | volume nominal | não |
| G — DESCRIÇÃO | `Posto / T1 – GC 15 m³` | não |
| H — **MEDIÇÃO (cm)** | leitura da régua | **sim** |
| I — DATA DA LEITURA | opcional (padrão: último dia do trimestre) | opcional |
| J — OBSERVAÇÃO | livre | opcional |

A célula B2 guarda o nº do inventário; a importação recusa planilha de outro inventário.

## Coleta pelo WhatsApp (computer use)
- Buscar as mensagens com as leituras do último dia do trimestre em cada grupo/conversa de posto.
- Para cada posto, casar cada leitura com a linha pela coluna DESCRIÇÃO (combustível + capacidade).
- Aceita `123,5`, `123.5` ou `123,5 cm`. Tanque sem leitura fica em branco e aparece listado após a importação.
- Leitura maior que o diâmetro do tanque gera aviso (provável erro de digitação).
