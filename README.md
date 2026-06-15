# BoxManager - Gestionale per Scatolifici
Progetto per il Laboratorio di Interfaccia Uomo-Macchina (IUM)
Alma Mater Studiorum - Università di Bologna (Campus di Cesena)

Presentato da: Niki Hammond & Silvia Busti
Docente: Prof. Andrea Bianchi


## Contesto e Obiettivi del Progetto
La gestione delle comunicazioni all'interno delle aziende manifatturiere presenta spesso criticità legate alla frammentazione dei dati. Preventivi, schede tecniche e stati di produzione vengono frequentemente gestiti tramite strumenti disgiunti (fogli di calcolo, email), incrementando il rischio di errori e ritardi. 

BoxManager si propone di unificare questi processi in un'unica piattaforma web centralizzata. L'applicativo fornisce all'amministratore strumenti per l'organizzazione del flusso di lavoro e garantisce al cliente un portale trasparente per il tracciamento dei propri ordini. L'obiettivo primario è dimostrare come l'ottimizzazione dell'usabilità e la centralizzazione dei dati possano abbattere i costi operativi e prevenire errori di produzione.


## Descrizione del Software
BoxManager è un Minimum Viable Product (MVP) di un sistema gestionale web-based progettato per produttori di scatole in cartone ondulato. Al fine di garantire la sicurezza dei dati aziendali, il sistema richiede un'autenticazione obbligatoria per l'accesso. Questo approccio previene accessi non autorizzati e consente di erogare interfacce differenziate (Amministratore o Cliente) in base al ruolo dell'utente autenticato.


## Caratteristiche Principali
- **Dashboard Direzionale e Multi-Ruolo:** Panoramica su ordini, clienti e fatturato riservata agli amministratori. I clienti visualizzano una dashboard essenziale filtrata esclusivamente sui propri dati aziendali.
- **Anagrafica Clienti (CRUD):** Gestione integrata dei dati aziendali, dei referenti, delle note e dello storico ordini.
- **Gestione Ordini (SQLite & EF Core):** Tracciamento degli ordini basato su database relazionale con approccio *Code-First*.
- **Scheda Tecnica Dettagliata:** Memorizzazione delle specifiche fisiche (dimensioni L×W×H, codice FEFCO, tipo onda) e di stampa (tecnologia, colori e finiture).
- **Sincronizzazione Real-time:** Utilizzo di SignalR e Vue.js per l'aggiornamento istantaneo degli stati di produzione tra tutti gli utenti connessi, eliminando la necessità di ricaricare la pagina.


## Ottimizzazioni UX e Principi IUM Applicati
Il design dell'interfaccia è stato sviluppato applicando i principi dell'Interazione Uomo-Macchina per minimizzare il carico cognitivo dell'utente:
- **Stepper Visivo di Avanzamento:** Lo stato dell'ordine è rappresentato tramite uno *stepper orizzontale* dinamico, offrendo un feedback visivo immediato della filiera produttiva.
- **Interfacce Adattive (Role-Based UI):** I comandi operativi critici ("Modifica", "Avvia produzione") sono limitati all'Amministratore. La loro rimozione dalla vista Cliente risponde al principio di prevenzione degli errori e riduce la complessità dell'interfaccia.
- **Accesso Vincolato a Credenziali:** Il sistema replica gli standard di sicurezza B2B, assicurando che le dinamiche industriali siano accessibili unicamente alle parti interessate.
- **Popolamento Realistico del Database:** L'applicativo implementa un modulo `DbInitializer` che auto-popola le tabelle simulando ordini storici con specifiche industriali verosimili (es. codici FEFCO, cartone a doppia onda), offrendo un ambiente di test consistente in fase di valutazione.


## Avvio del Progetto e Credenziali di Test
L'esecuzione in ambiente locale richiede l'installazione del `.NET SDK`. 
1. Clonare la repository.
2. Posizionarsi nella cartella principale del progetto da terminale.
3. Eseguire il comando `dotnet run`.

Al primo avvio, l'applicativo genera automaticamente il database locale `BoxManager.db` e inserisce i dati di test preconfigurati.

Per la valutazione del progetto, il sistema di login permette l'accesso inserendo una **qualsiasi password** in combinazione con i seguenti account:
- **Vista Amministratore (Controllo Totale):** `admin@boxmanager.it`
- **Vista Cliente (Sola Lettura Filtrata):** `c.bianchi@outlook.it` (oppure `v.ferrari@outlook.com`)


## Conclusioni
L'applicativo dimostra l'integrazione tra logiche di business backend e l'applicazione rigorosa delle euristiche di usabilità affrontate nel corso IUM. L'utilizzo di tecnologie come SignalR, interfacce adattive e workflow visivi chiari permette di trasformare un software gestionale industriale in uno strumento intuitivo, ottimizzando in maniera tangibile l'interazione uomo-macchina.
