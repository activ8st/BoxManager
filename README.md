# BoxManager - Gestionale per Scatolifici
Progetto per il Laboratorio di Interfaccia Uomo-Macchina (IUM)
Alma Mater Studiorum - Università di Bologna (Campus di Cesena)

Docente: Prof. Andrea Bianchi


## Descrizione del Progetto
BoxManager è un Minimum Viable Product (MVP) di un sistema gestionale web-based progettato specificamente per aziende produttrici di scatole in cartone ondulato. L'obiettivo principale è centralizzare e ottimizzare il flusso di lavoro che intercorre tra l'acquisizione del cliente, la definizione della scheda tecnica e la gestione della produzione.
Il progetto si focalizza sull'usabilità (UX) e sulla coerenza dell'interfaccia (UI), fornendo strumenti dinamici per il calcolo dei preventivi e il monitoraggio in tempo reale degli stati di avanzamento degli ordini.


## Caratteristiche Principali
Dashboard Direzionale: Panoramica immediata su ordini totali, clienti attivi e fatturato stimato, con widget per la ricerca rapida e gli ultimi ordini inseriti.
Anagrafica Clienti (CRUD): Gestione completa dei dati aziendali, referenti e note specifiche, con storico ordini integrato per ogni singola scheda cliente.
Gestione Ordini e Workflow: Sistema di tracciamento degli ordini con stati dinamici (In attesa, In produzione, Completato, Consegnato).
 Scheda Tecnica Dettagliata: Definizione delle caratteristiche fisiche (dimensioni L×W×H, codice FEFCO, tipo onda) e specifiche di stampa (offset/flexo, colori, finiture).
Real-time & Dinamicità:
SignalR: Sincronizzazione istantanea dello stato degli ordini tra diversi client connessi.
Vue.js: Motore di calcolo lato client per l'aggiornamento in tempo reale del totale ordine (quantità, prezzo unitario e sconti) senza ricaricamento della pagina.


## Stack Tecnologico
Backend: ASP.NET Core 8.0 MVC
ORM: Entity Framework Core con approccio Code-First
Database: SQLite (file-based, ideale per portabilità e debug locale)
Frontend: Bootstrap 5, Vue.js 3 (Progressive Framework), SignalR
Iconografia: Bootstrap Icons


## Requisiti di Sistema
.NET 8.0 SDK
Visual Studio Code (consigliato) o Visual Studio 2022
Istruzioni per l'Esecuzione
Navigazione: Aprire il terminale nella cartella radice del progetto (BoxManager).
Ripristino Dipendenze:
Bash
dotnet restore
Compilazione ed Esecuzione:
Bash
dotnet run
Accesso: L'applicazione sarà disponibile di default su https://localhost:5001 o http://localhost:5000.
Nota: Il database SQLite (BoxManager.db ) viene generato e popolato automaticamente con dati di test al primo avvio tramite la classe DbInitializer.


## Struttura del Progetto
/Controllers: Logica di gestione delle richieste e coordinamento tra modelli e viste.
/Models: Definizioni delle entità di dominio (Customer, Order, TechnicalSheet).
/Data: Context di Entity Framework e logica di inizializzazione del DB.
/Views: Template Razor per l'interfaccia utente, organizzati per moduli.
/Hubs: Configurazione SignalR per la comunicazione bidirezionale.
/wwwroot: Asset statici (CSS, JS, immagini)

Presentato da Silvia Busti e Niki Hammond.
