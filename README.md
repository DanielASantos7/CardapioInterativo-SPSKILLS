# CardapioInterativo - SPSKILLS

Projeto desenvolvido para a competição SPSKILLS na modalidade Desenvolvimento de Aplicativos de Software. A aplicação consiste em um sistema composto por uma Web API RESTful para persistência e regras de negócio, integrada a um aplicativo Mobile cross-platform para proprietários e clientes, desenvolvido sob restrições de tempo e requisitos operacionais.

## Condições da Prova

- **Tempo máximo de execução:** 3 horas (180 minutos)
- **Escopo:** Web API, aplicativo Mobile (.NET MAUI), autenticação por perfil, regras de negócio e integração com banco de dados
- **Modalidade:** Desenvolvimento de Aplicativos de Software
- **Módulo:** Aplicação Mobile Cross-Platform & Web API RESTful

## Tecnologias Utilizadas

- C#
- .NET 8.0 (Web API)
- .NET MAUI (Mobile)
- Entity Framework Core 8 (Database First / Scaffolding)
- SQL Server (SQL Authentication)

## Funcionalidades Implementadas

- **Tela Splash:** Temporizador de 12 segundos com transição dinâmica de cores oficiais sem repetição.
- **Sistema de Autenticação:** Validação de credenciais via API e controle de acesso baseado em perfis (Proprietário e Cliente).
- **Gestão de Restaurantes:** Listagem alfabética, busca reativa a partir do 2º caractere, tratamento de fotos nulas, Soft Delete via clique longo e restauração de registros.
- **Cardápio Interativo:** Mapeamento de pratos, definição de preços via modal (Proprietário), ordenação por categoria e persistência de curtidas (Cliente).
- **Ranking Top 3 Interativo:** Apresentação visual dos restaurantes mais populares, alternância de posições ao clique e animação de virada de card.

## Estrutura do Projeto

- **CardapioInterativo.API:** Projeto Web API responsável pelo mapeamento do banco de dados, endpoints de autenticação e regras de negócio.
- **CardapioInterativo.MOBILE:** Aplicativo .NET MAUI responsável pela interface gráfica, consumo dos endpoints RESTful e interações do usuário.
- **appsettings.json:** Configurações de infraestrutura e string de conexão com a instância do SQL Server.

## Como Executar

1. Certifique-se de que a base de dados `dbCardapio` esteja disponível na instância do SQL Server indicada na prova.
2. Abra a solução `CardapioInterativo.sln` no Visual Studio.
3. Se necessário, ajuste os parâmetros de servidor, usuário e senha da string de conexão `DefaultConnection` no arquivo `appsettings.json` do projeto `CardapioInterativo.API`.
4. Compile a solução utilizando `Ctrl + Shift + B`.
5. Execute o projeto `CardapioInterativo.API` (`Ctrl + F5`) para disponibilizar as rotas da aplicação.
6. Execute o projeto `CardapioInterativo.MOBILE` no emulador ou dispositivo de destino.
