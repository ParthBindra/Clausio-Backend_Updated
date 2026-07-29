using System.Text;
using Clausio.Legal.Core.Dtos;
using Clausio.Legal.Infrastructure;
using Clausio.Legal.Infrastructure.Ai;
using Microsoft.EntityFrameworkCore;

namespace Clausio.Legal.Service;

public interface IAiService
{
    Task<string> SummarizeCaseAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> GenerateChronologyAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> DetectContradictionsAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> AnalyzeEvidenceAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<string> ResearchAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> GenerateActionPlanAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> TranslateAsync(TranslateRequest request, CancellationToken cancellationToken = default);
    Task<string> ChatAsync(ChatRequestDto request, CancellationToken cancellationToken = default);
    Task<string> DraftWhatsAppAsync(Guid caseId, WhatsAppRequestDto request, CancellationToken cancellationToken = default);
    Task<string> AnalyzeFinancialsAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> AssessReadinessAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> EmergencyTriageAsync(EmergencyRequestDto request, CancellationToken cancellationToken = default);
    Task<string> PrepHearingAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> PrepWitnessAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<string> ClassifyCaseTypeAsync(CaseTypeRequestDto request, CancellationToken cancellationToken = default);
    Task<string> DraftDocumentAsync(Guid caseId, DraftRequestDto request, CancellationToken cancellationToken = default);
}

public class AiService(ClausioDbContext db, IAiClient aiClient) : IAiService
{
    // ============================================================
    // MASTER IDENTITY — Who Clausio AI is
    // ============================================================
    private const string MasterIdentity =
        "You are the AI engine of Clausio, India's professional litigation intelligence platform. " +
        "You function as a senior junior advocate — the best junior advocate a Senior Counsel has ever had. " +
        "Your work is relied upon by practising advocates across India in real courts — " +
        "Family Courts, District Courts, Sessions Courts, High Courts, and the Supreme Court of India. " +
        "\n\n" +
        "YOUR KNOWLEDGE BASE IS EXHAUSTIVE: " +
        "\n" +
        "SUBSTANTIVE LAW: IPC 1860, BNS 2023, CrPC 1973 / BNSS 2023, CPC 1908, " +
        "Indian Evidence Act 1872 / BSA 2023, Hindu Marriage Act 1955, " +
        "Special Marriage Act 1954, Hindu Succession Act 1956, " +
        "Protection of Women from Domestic Violence Act 2005, " +
        "Maintenance and Welfare of Parents and Senior Citizens Act 2007, " +
        "Guardians and Wards Act 1890, Juvenile Justice Act 2015, " +
        "Transfer of Property Act 1882, Registration Act 1908, " +
        "Specific Relief Act 1963, Limitation Act 1963, " +
        "Negotiable Instruments Act 1881 (especially Section 138), " +
        "Contract Act 1872, Sale of Goods Act 1930, " +
        "Consumer Protection Act 2019, RERA 2016, " +
        "Companies Act 2013, Insolvency and Bankruptcy Code 2016, " +
        "PMLA 2002, FEMA 1999, " +
        "Income Tax Act 1961, GST Acts 2017, " +
        "Arbitration and Conciliation Act 1996, " +
        "Labour laws: ID Act 1947, Factories Act 1948, ESIC Act 1948, EPF Act 1952, " +
        "Constitution of India — especially Part III (Articles 12-35), Part IV-A, Article 136, 226, 227, " +
        "RTI Act 2005, IT Act 2000, POCSO Act 2012, SC/ST (Prevention of Atrocities) Act 1989. " +
        "\n\n" +
        "PROCEDURAL EXPERTISE: " +
        "Supreme Court Rules 2013, High Court Rules of all 25 High Courts, " +
        "CPC Orders I-LI with all Rules, " +
        "Criminal procedure under CrPC and BNSS including bail, trial, appeal, revision, " +
        "Family Court Act 1984 and procedure, " +
        "Limitation periods under Schedule to Limitation Act 1963, " +
        "Court fees under Court Fees Act 1870, " +
        "Stamp duty requirements under Indian Stamp Act 1899. " +
        "\n\n" +
        "LANDMARK JUDGMENTS: You know every significant Supreme Court and High Court judgment " +
        "from A.K. Gopalan v. State of Madras (1950) to the present day. " +
        "You cite accurately — court name, year, volume, page number. " +
        "You NEVER fabricate citations. If unsure, you say 'verify citation before filing.' " +
        "\n\n" +
        "OUTPUT STANDARD: " +
        "Every output must be of the quality that a Senior Advocate at a top-tier Indian law firm " +
        "would be proud to sign and submit to court. " +
        "Be thorough, precise, and practical. " +
        "A junior advocate preparing a 30-page draft should produce a 30-page draft. " +
        "A cross-examination should have 25-30 questions, not 5. " +
        "An action plan should cover everything, not just obvious items. " +
        "\n\n" +
        "CRITICAL OUTPUT FORMAT RULE: " +
        "Return valid JSON only — no markdown, no backticks, no preamble. " +
        "Exception: drafting endpoint returns plain text formatted document. " +
        "Exception: chat endpoint returns plain conversational text. " +
        "Exception: emergency endpoint returns plain structured text.";

    // ============================================================
    // 1. CASE SUMMARY — Full senior partner brief
    // ============================================================
    public Task<string> SummarizeCaseAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId,
            "Prepare a comprehensive case summary brief as if you are the lead junior advocate " +
            "briefing a Senior Counsel who is appearing in this case for the first time tomorrow. " +
            "The Senior Counsel must walk into court knowing everything about this case from your brief alone. " +
            "\n\n" +
            "Your brief must cover: " +
            "1. Who the parties are and their relationship " +
            "2. What exactly happened — chronologically with specific dates " +
            "3. What legal relief is being sought and under which sections " +
            "4. What has happened in court so far — every hearing, every order " +
            "5. What evidence we have — document by document " +
            "6. What our strongest arguments are with section and judgment references " +
            "7. What weaknesses exist and how to handle them " +
            "8. What the opposing party's likely defence will be " +
            "9. What must happen next — specific actions with deadlines " +
            "10. Your honest assessment of the case with probability " +
            "\n\n" +
            "Return ONLY this comprehensive JSON — every field must be detailed, not generic: " +
            "{" +
            "\"coreFacts\": \"3-4 sentences: who, what, when, where — specific dates and facts from the dossier\", " +
            "\"currentStage\": \"Exact procedural stage, last hearing date, what order was passed, what is pending\", " +
            "\"partiesProfile\": {\"petitioner\": \"Who they are, occupation, age if known\", \"respondent\": \"Who they are, occupation, what they have done\"}, " +
            "\"causeOfAction\": \"The specific legal wrong that has occurred and when it arose\", " +
            "\"reliefSought\": [\"Specific relief 1 with section\", \"Relief 2\", \"Relief 3\"], " +
            "\"applicableSections\": [{\"section\": \"Section 13(1)(ia) HMA 1955\", \"purpose\": \"Ground of cruelty for divorce\"}, {\"section\": \"Section 25 HMA\", \"purpose\": \"Permanent alimony\"}], " +
            "\"evidenceSummary\": [{\"document\": \"Hospital Discharge Summary\", \"proves\": \"Physical assault on 12 Aug 2020\", \"exhibit\": \"Exhibit A\", \"strength\": \"Strong\"}], " +
            "\"keyStrengths\": [{\"strength\": \"Specific strength\", \"basis\": \"Evidence or section that supports it\", \"impactLevel\": \"High\"}], " +
            "\"keyWeaknesses\": [{\"weakness\": \"Specific weakness\", \"risk\": \"How opposing counsel will exploit this\", \"mitigation\": \"How to address this\"}], " +
            "\"opposingCaseAnticipated\": [\"Anticipated defence argument 1 with counter\", \"Defence argument 2 with counter\"], " +
            "\"keyJudgments\": [{\"citation\": \"Rajnesh v. Neha (2020) 14 SCC 1\", \"relevance\": \"Why applicable\", \"paragraph\": \"Para 47\"}], " +
            "\"nextSteps\": [{\"action\": \"Specific action\", \"deadline\": \"YYYY-MM-DD\", \"responsiblePerson\": \"Advocate/Client/Clerk\", \"urgency\": \"Critical\"}], " +
            "\"verdictProbability\": {\"favorable\": 68, \"partial\": 22, \"adverse\": 10, \"basis\": \"Why you assess this probability\"}, " +
            "\"caseKiller\": \"The single biggest risk that could lose this case — be honest\", " +
            "\"seniorCounselBrief\": \"A professional 8-10 sentence comprehensive summary suitable for a Senior Counsel's brief — written in the third person, past tense for facts, present tense for legal position\"" +
            "}",
            cancellationToken);

    // ============================================================
    // 2. CHRONOLOGY — Court-ready timeline
    // ============================================================
    public Task<string> GenerateChronologyAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId,
            "Construct a comprehensive, court-ready chronological timeline of this case. " +
            "This timeline will be submitted as a typed exhibit in court to establish the narrative. " +
            "A judge reading this timeline must be able to understand the entire case history " +
            "without reading any other document. " +
            "\n\n" +
            "Rules for this timeline: " +
            "1. Every event must have a specific date — no vague 'sometime in 2020' " +
            "2. Every event must reference the document/exhibit that proves it " +
            "3. Events must establish a clear narrative showing the legal wrong " +
            "4. Include court dates, orders, and procedural events " +
            "5. Mark events that are disputed by the opposing party " +
            "6. Flag events that need additional documentary proof " +
            "\n\n" +
            "Return ONLY a comprehensive JSON array — minimum 15 events if data supports: " +
            "[{" +
            "\"serialNo\": 1, " +
            "\"eventDate\": \"YYYY-MM-DD\", " +
            "\"eventDateDisplay\": \"15th February 2015\", " +
            "\"event\": \"Precise factual description — specific, not generic\", " +
            "\"category\": \"Marriage|Cruelty|Physical Assault|Verbal Abuse|Financial Fraud|Property|Court Filing|Hearing|Order|Medical|Police|Other\", " +
            "\"documentProof\": \"Marriage Certificate / Hospital Record dated 12.08.2020 / Bank Statement / Court Order\", " +
            "\"exhibitLabel\": \"Exhibit A\", " +
            "\"legalSignificance\": \"This event establishes [cruelty under Section 13(1)(ia) HMA] / [desertion under Section 13(1)(ib)] / etc.\", " +
            "\"isDisputed\": false, " +
            "\"advantageTo\": \"Petitioner|Respondent|Neutral\", " +
            "\"witnessWhoCanProve\": \"Dr. Mehta / Petitioner herself / Bank employee\", " +
            "\"needsMoreProof\": false, " +
            "\"proofGap\": \"What additional document would make this event unassailable\"" +
            "}]",
            cancellationToken);

    // ============================================================
    // 3. CONTRADICTIONS — Forensic inconsistency analysis
    // ============================================================
    public Task<string> DetectContradictionsAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId,
            "Conduct a forensic contradiction analysis of all evidence, statements, and documents in this case. " +
            "You are functioning as a specialist contradiction analyst whose job is to destroy the opposing party's case " +
            "by identifying every single inconsistency, discrepancy, lie, and contradiction. " +
            "\n\n" +
            "Apply these legal principles: " +
            "Section 145 Indian Evidence Act — prior inconsistent statements " +
            "Section 155 Indian Evidence Act — impeaching credit of witness " +
            "Section 146 Indian Evidence Act — questions lawful in cross-examination " +
            "Section 118 Indian Evidence Act — competency of witnesses " +
            "\n\n" +
            "Look for contradictions in: " +
            "1. Statements vs documents (what they said vs what documents show) " +
            "2. Financial declarations vs actual lifestyle/assets " +
            "3. Timeline contradictions (two events claimed at same time) " +
            "4. Witness vs witness (different witnesses saying different things) " +
            "5. Previous applications vs current stand " +
            "6. WhatsApp/social media vs sworn statements " +
            "\n\n" +
            "Return ONLY a comprehensive JSON array — be exhaustive, not minimal: " +
            "[{" +
            "\"serialNo\": 1, " +
            "\"category\": \"Financial Contradiction|Timeline Contradiction|Statement Contradiction|Document Contradiction|Witness Contradiction\", " +
            "\"claimByOpposingParty\": \"Exact claim made by opposing party — quote verbatim if possible\", " +
            "\"claimSource\": \"Written Statement Para 5 / Affidavit dated 15.03.2024 page 3 / Statement before IO\", " +
            "\"contradictingEvidence\": \"The specific evidence that directly and irrefutably contradicts this claim\", " +
            "\"evidenceSource\": \"Bank Statement HDFC dated 01.03.2024 showing credit of Rs 45L / BMW RC Book / Hospital Record\", " +
            "\"exactContradiction\": \"Opposing party claims income is Rs 22L/year but purchased BMW X5 worth Rs 45L in March 2024 — this is impossible on declared income\", " +
            "\"legalBasis\": \"Section 145 Evidence Act — prior inconsistent statement admissible to impeach credit\", " +
            "\"crossExaminationQuestions\": [" +
            "\"Q: You have stated in your affidavit that your monthly income is Rs 22,000. Is that correct?\", " +
            "\"Q: I am now showing you a document marked Exhibit X — this is the RC Book of a BMW X5 registered in your name. Is this your vehicle?\", " +
            "\"Q: The ex-showroom price of this vehicle is Rs 45 lakhs. How did you purchase this vehicle on a monthly income of Rs 22,000?\"" +
            "], " +
            "\"courtArgument\": \"My Lord, the Respondent's claim of earning Rs 22,000 per month is demonstrably false as evidenced by Exhibit X which shows purchase of BMW X5 worth Rs 45 lakhs.\", " +
            "\"impactIfUsed\": \"Completely destroys credibility of Respondent's income affidavit — opens door for financial discovery\", " +
            "\"strength\": \"Devastating|Very High|High|Medium|Low\", " +
            "\"howToPreserveForTrial\": \"File certified copy of RC Book with Section 65B certificate if from electronic records\"" +
            "}]",
            cancellationToken);

    // ============================================================
    // 4. EVIDENCE ANALYSIS — Document by document
    // ============================================================
    public async Task<string> AnalyzeEvidenceAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await db.Documents.Include(d => d.Case)
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        var systemPrompt = MasterIdentity +
            "\n\nYou are now functioning as a documentary evidence specialist under the Indian Evidence Act 1872. " +
            "You are analysing: " + document.FileName +
            " | Document Type: " + document.DocumentType +
            " | Case: " + document.Case?.Name +
            " | Exhibit: " + (document.ExhibitLabel ?? "Not yet labelled") +
            "\n\n" +
            "Apply these Evidence Act provisions rigorously: " +
            "Section 61-66: Production and proof of documents " +
            "Section 65B: Admissibility of electronic records (mandatory certificate for digital docs) " +
            "Section 74-78: Public documents " +
            "Section 79-90: Presumptions as to documents " +
            "Section 45-51: Expert opinion " +
            "Section 34: Entries in books of account " +
            "Section 32: Statements of persons who cannot be called as witnesses";

        var userPrompt =
            "Conduct a comprehensive forensic legal analysis of this document as evidence. " +
            "Your analysis will determine how this document is used in court. " +
            "Return ONLY this detailed JSON: " +
            "{" +
            "\"documentProfile\": {\"type\": \"Public|Private|Electronic|Expert Report\", \"author\": \"Who created this\", \"date\": \"YYYY-MM-DD\", \"authenticatedBy\": \"Notary/Registrar/Hospital/Bank\"}, " +
            "\"whatItProves\": [\"Specific fact 1 with page/para/line reference\", \"Fact 2\", \"Fact 3\", \"Fact 4\"], " +
            "\"admissibility\": {\"isAdmissible\": true, \"primarySection\": \"Section 65 Evidence Act\", \"conditionsForAdmission\": \"Must be produced through the person who issued it\", \"section65BRequired\": false, \"section65BCertificateObtained\": false}, " +
            "\"evidenceStrength\": \"Conclusive|Very Strong|Strong|Moderate|Weak|Inadmissible\", " +
            "\"exhibitStrategy\": {\"recommendedLabel\": \"Exhibit A\", \"throughWhichWitness\": \"Dr. Mehta who issued the discharge summary\", \"howToTender\": \"Hand original + 3 certified copies to court, retain one for file\", \"whenToTender\": \"During examination-in-chief of the issuing authority\"}, " +
            "\"anticipatedObjections\": [{\"objection\": \"Objection: Document not proved\", \"counter\": \"We will call the issuing authority to prove this document under Section 67 Evidence Act\"}], " +
            "\"keyHighlights\": [\"Para 3: Date of admission confirms incident occurred on 12.08.2020\", \"Para 7: Injuries consistent with assault\"], " +
            "\"useInArguments\": [\"Use to establish date of assault\", \"Use to establish nature and extent of injury\", \"Use to corroborate petitioner's testimony\"], " +
            "\"documentGaps\": \"What is missing from this document that would make it more powerful\", " +
            "\"additionalDocumentsNeeded\": [\"Obtain treating doctor's affidavit\", \"Obtain certified copy from hospital records\"], " +
            "\"digitalAuthenticity\": \"If WhatsApp screenshot or digital — must comply with Section 65B — certificate from phone owner required\", " +
            "\"relatedSections\": [\"Section 65 Evidence Act\", \"Section 74 Evidence Act\", \"Section 79 Evidence Act\"]" +
            "}";

        return await aiClient.CompleteAsync(systemPrompt, userPrompt, cancellationToken);
    }

    // ============================================================
    // 5. LEGAL RESEARCH — Binding and persuasive judgments
    // ============================================================
    public Task<string> ResearchAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId,
            "Conduct comprehensive legal research for this case as a Supreme Court law clerk would. " +
            "You must find the best possible judgments across ALL relevant legal issues in this case. " +
            "\n\n" +
            "Research must cover: " +
            "1. The main cause of action — what sections apply and key cases " +
            "2. The relief sought — judgments establishing the court's power to grant this relief " +
            "3. Quantum judgments — what courts have awarded in similar cases " +
            "4. Procedural judgments — any procedural advantages from past decisions " +
            "5. Counter to anticipated defences — cases that defeat likely defence arguments " +
            "6. Recent judgments — cases from last 5 years showing current judicial trend " +
            "\n\n" +
            "CITATION RULE: Only cite cases you are certain exist. " +
            "For each case include the paragraph number containing the key ratio. " +
            "Distinguish between binding (Supreme Court) and persuasive (High Court) authority. " +
            "\n\n" +
            "Return ONLY a comprehensive JSON array of 6-8 judgments: " +
            "[{" +
            "\"serialNo\": 1, " +
            "\"citation\": \"Exact citation: Rajnesh v. Neha (2020) 14 SCC 1\", " +
            "\"shortName\": \"Rajnesh v. Neha\", " +
            "\"court\": \"Supreme Court of India|Delhi High Court|Bombay High Court\", " +
            "\"year\": 2020, " +
            "\"coram\": \"Justice DY Chandrachud, Justice Indu Malhotra\", " +
            "\"bindingOrPersuasive\": \"Binding on all courts in India\", " +
            "\"legalIssueAddressed\": \"The specific legal question answered by this case\", " +
            "\"ratioDecidendi\": \"The exact legal principle/ratio in 2-3 sentences — what the court held\", " +
            "\"keyParagraph\": \"Para 47 and Para 52\", " +
            "\"exactQuoteForArgument\": \"The court held: [exact language from the judgment relevant to our case]\", " +
            "\"howToUseInOurCase\": \"Specifically, in oral arguments say: My Lord, in Rajnesh v. Neha at Para 47, the Supreme Court held that...\", " +
            "\"defenceArgumentItDefeats\": \"Opposing counsel will argue X — this case defeats that by holding Y\", " +
            "\"factualSimilarity\": \"How the facts of this case mirror our case — High/Medium/Low\", " +
            "\"strength\": \"Decisive|Very Strong|Strong|Useful\", " +
            "\"indianKanoonLink\": null, " +
            "\"distinguishedIfNecessary\": \"If opposing counsel tries to distinguish — respond by saying...\"" +
            "}]",
            cancellationToken);

    // ============================================================
    // 6. 30-DAY ACTION PLAN — Strategic litigation roadmap
    // ============================================================
    public Task<string> GenerateActionPlanAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId,
            "Create a comprehensive, detailed 30-day litigation action plan for this case. " +
            "You are functioning as the Senior Advocate who has just reviewed the case file " +
            "and is now dictating instructions to the junior team. " +
            "\n\n" +
            "The action plan must cover EVERYTHING: " +
            "1. Immediate actions (within 48 hours) — urgent filings, urgent applications " +
            "2. Evidence gathering — every document that needs to be obtained " +
            "3. Witness management — who to contact, what to obtain, how to prepare " +
            "4. Court filings — applications, replies, written arguments " +
            "5. Client instructions — what information/documents client must provide " +
            "6. Research tasks — what needs to be researched " +
            "7. Administrative tasks — court fees, certified copies, stamps " +
            "8. Deadlines tracking — all pending orders and their due dates " +
            "\n\n" +
            "Do NOT include generic items like 'prepare for hearing' — every item must be specific to THIS case. " +
            "\n\n" +
            "Return ONLY a comprehensive JSON array — minimum 10 items, maximum 15: " +
            "[{" +
            "\"serialNo\": 1, " +
            "\"title\": \"File Application for Discovery and Inspection of Respondent's ITR and Bank Statements\", " +
            "\"description\": \"Prepare and file an application under Order XI Rule 12 CPC seeking discovery of Respondent's ITR for last 5 years, HDFC Bank account statements, and property documents. Grounds: Respondent has concealed income as evidenced by BMW purchase. Prayer: Direct Respondent to produce within 7 days.\", " +
            "\"legalBasis\": \"Order XI Rule 12 CPC — Discovery of documents / Section 91 CrPC — Production of documents\", " +
            "\"priority\": \"Critical|High|Medium|Low\", " +
            "\"dueBy\": \"YYYY-MM-DD\", " +
            "\"assignedTo\": \"Senior Advocate|Junior Advocate|Client|Clerk\", " +
            "\"category\": \"Court Filing|Evidence Gathering|Witness Management|Client Instruction|Research|Administrative\", " +
            "\"specificInstructions\": \"Draft the application in 3 parts: (1) Background facts — para 1-3, (2) Documents sought with specificity, (3) Prayer. Attach Exhibit X as proof of income concealment.\", " +
            "\"consequenceIfNotDone\": \"Without Respondent's financial documents, income concealment argument will be circumstantial only — this application strengthens the financial case substantially\", " +
            "\"estimatedTime\": \"3 hours to draft, 1 hour to file\", " +
            "\"dependencies\": \"Requires certified copy of BMW RC Book — obtain from RTO first\", " +
            "\"successCriteria\": \"Application filed, copy served on Respondent's advocate, date obtained for hearing\"" +
            "}]",
            cancellationToken);

    // ============================================================
    // 7. TRANSLATE
    // ============================================================
    public Task<string> TranslateAsync(TranslateRequest request, CancellationToken cancellationToken = default) =>
        aiClient.CompleteAsync(
            MasterIdentity +
            "\n\nYou are now acting as a certified court interpreter with expertise in " +
            "Hindi, Gujarati, Marathi, Tamil, Telugu, Kannada, Bengali, and English. " +
            "You translate legal documents preserving all legal terminology exactly. " +
            "Section references, case citations, legal terms of art — these are NEVER translated, they stay as-is. " +
            "When translating to English, use formal legal English. " +
            "When translating to Indian languages, use formal legal register of that language.",
            "Translate the following text. Detect source language automatically. " +
            "Return ONLY this JSON: " +
            "{\"translatedText\": \"complete professional translation\", " +
            "\"detectedLanguage\": \"Hindi|Gujarati|Marathi|Tamil|Telugu|English|Kannada|Bengali\", " +
            "\"originalText\": \"original text verbatim\", " +
            "\"legalTermsKeptAsIs\": [\"Terms not translated — kept in original\"], " +
            "\"translationNotes\": \"Any ambiguities or important notes\", " +
            "\"formalityLevel\": \"Court document|Legal notice|Client communication|WhatsApp\"} " +
            "\n\nText to translate:\n" + (request.Text ?? ""),
            cancellationToken);

    // ============================================================
    // 8. AI CHAT — Senior colleague consultation
    // ============================================================
    public async Task<string> ChatAsync(ChatRequestDto request, CancellationToken cancellationToken = default)
    {
        var systemPrompt = MasterIdentity +
            "\n\nYou are now in direct consultation mode with a practising Indian advocate. " +
            "Respond like the most knowledgeable senior colleague they have ever consulted. " +
            "Be direct, specific, and practical. Reference exact sections, rules, and judgments. " +
            "If asked about a specific case procedure — walk through it step by step. " +
            "If asked about drafting — provide the actual draft language. " +
            "If asked about a judgment — cite it accurately with paragraph numbers. " +
            "Never be vague. Never say 'it depends' without explaining what it depends on. " +
            "Respond in plain text — this is a chat interface, not a JSON output.";

        if (request.CaseId is Guid caseId)
        {
            var dossier = await BuildCaseDossierAsync(caseId, cancellationToken);
            systemPrompt += "\n\nCURRENT CASE CONTEXT (use to give specific answers):\n" + dossier;
        }

        var sb = new StringBuilder();
        if (request.History is { Count: > 0 })
        {
            for (var i = 0; i < request.History.Count; i++)
                sb.AppendLine((i % 2 == 0 ? "Advocate: " : "Clausio: ") + request.History[i]);
        }
        sb.Append("Advocate: ").Append(request.Message);

        return await aiClient.CompleteAsync(systemPrompt, sb.ToString(), cancellationToken);
    }

    // ============================================================
    // 9. WHATSAPP MESSAGE — Professional client communication
    // ============================================================
    public Task<string> DraftWhatsAppAsync(Guid caseId, WhatsAppRequestDto request, CancellationToken cancellationToken = default)
    {
        var tone     = request.Tone     ?? "Professional";
        var language = request.Language ?? "English";

        return RunWithCaseContextAsync(caseId,
            "Draft a professional WhatsApp message to the client about their case. " +
            "\n\n" +
            "REQUIREMENTS: " +
            "Language: " + language + " " +
            "Tone: " + tone + " " +
            "Length: Maximum 250 words — clients do not read long messages " +
            "Format: Proper WhatsApp format with line breaks, not a wall of text " +
            "\n\n" +
            "MUST INCLUDE: " +
            "1. Greeting by client's first name " +
            "2. What happened in the most recent hearing — specifically " +
            "3. What the judge said or ordered " +
            "4. Next hearing date " +
            "5. One specific action the client must take before the next hearing " +
            "6. Reassurance that the matter is being handled professionally " +
            "7. Advocate's name at the end " +
            "\n\n" +
            "MUST NOT INCLUDE: " +
            "Case strategy, weaknesses, confidential information, legal jargon, " +
            "anything that could create panic or false expectations " +
            "\n\n" +
            "Return ONLY this JSON: " +
            "{" +
            "\"message\": \"Complete WhatsApp message ready to copy-paste and send\", " +
            "\"language\": \"" + language + "\", " +
            "\"tone\": \"" + tone + "\", " +
            "\"callToAction\": \"The specific one thing the client must do\"" +
            "}",
            cancellationToken);
    }

    // ============================================================
    // 10. FINANCIAL ANALYSIS — Rajnesh v. Neha standard
    // ============================================================
    public Task<string> AnalyzeFinancialsAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId,
            "Conduct a comprehensive forensic financial analysis for this matrimonial/financial dispute. " +
            "You are functioning as both a forensic accountant and a Senior Advocate specialising in " +
            "matrimonial finance in Indian courts. " +
            "\n\n" +
            "Apply the following legal standards: " +
            "1. Rajnesh v. Neha (2020) 14 SCC 1 — Standard of living test for maintenance " +
            "2. Jasbir Kaur Sehgal v. District Judge (1997) 7 SCC 7 — Maintenance quantum " +
            "3. Shamima Farooqui v. Shahid Khan (2015) 5 SCC 705 — Lifestyle evidence " +
            "4. Section 125 CrPC — Maintenance entitlement and computation " +
            "5. Section 24 HMA — Pendente lite maintenance " +
            "6. Section 26 HMA — Custody and maintenance of children " +
            "\n\n" +
            "IDENTIFY: " +
            "- All sources of income including undisclosed " +
            "- All assets including benami assets " +
            "- Income concealment methods " +
            "- Standard of living indicators " +
            "- Financial documentation available vs needed " +
            "\n\n" +
            "Return ONLY this comprehensive JSON: " +
            "{" +
            "\"declaredIncome\": 2200000, " +
            "\"estimatedActualIncome\": 4500000, " +
            "\"incomeRatio\": \"Estimated actual income is 2x declared income\", " +
            "\"incomeSources\": [{\"source\": \"Salary\", \"declared\": 2200000, \"estimated\": 3000000}, {\"source\": \"Business income\", \"declared\": 0, \"estimated\": 1500000}], " +
            "\"incomeConcealment\": [{\"method\": \"Cash business income not shown in ITR\", \"evidence\": \"BMW purchase on low declared income\", \"documentToObtain\": \"ITR for last 5 years via discovery application\"}], " +
            "\"assets\": [{\"asset\": \"BMW X5\", \"value\": 4500000, \"registeredIn\": \"Respondent's name\", \"exhibit\": \"Exhibit X\"}, {\"asset\": \"Flat at Powai\", \"value\": 12000000, \"registeredIn\": \"Father's name — benami\", \"exhibit\": null}], " +
            "\"standardOfLivingIndicators\": [\"BMW X5 ownership\", \"Annual foreign vacation\", \"Private school fees for children\", \"Club memberships\"], " +
            "\"suspiciousTransactions\": [\"Cash withdrawal of Rs 10L dated 01.03.2024 — one week before separation\", \"Transfer of flat to father dated 15.02.2024\"], " +
            "\"maintenanceComputation\": {" +
            "\"petitionerMonthlyNeeds\": 45000, " +
            "\"childrenMonthlyNeeds\": 30000, " +
            "\"totalNeed\": 75000, " +
            "\"respondentPayingCapacity\": 120000, " +
            "\"recommendedMaintenance\": 55000, " +
            "\"pendenteLiteApplication\": 40000, " +
            "\"minimumWeShouldAccept\": 35000, " +
            "\"maximumWeCanJustify\": 75000" +
            "}, " +
            "\"settlementComputation\": {" +
            "\"totalAssets\": 20000000, " +
            "\"petitionerShareEntitlement\": 5000000, " +
            "\"negotiationFloor\": 3500000, " +
            "\"negotiationCeiling\": 7000000, " +
            "\"recommendedSettlementAsk\": 5500000" +
            "}, " +
            "\"documentationRequired\": [\"ITR last 5 years via discovery\", \"Bank statements all accounts 3 years\", \"Property documents for Powai flat\", \"Car insurance papers showing premium paid\"], " +
            "\"keyJudgments\": [\"Rajnesh v. Neha (2020) 14 SCC 1 — Para 47: Standard of living test\", \"Jasbir Kaur Sehgal (1997) 7 SCC 7 — Maintenance must be reasonable\"], " +
            "\"courtArgument\": \"The specific 3-paragraph argument to make to the court about income concealment and maintenance quantum\", " +
            "\"summary\": \"3-4 sentence professional financial analysis suitable for written argument submission\"" +
            "}",
            cancellationToken);

    // ============================================================
    // 11. READINESS ASSESSMENT — Pre-hearing audit
    // ============================================================
    public async Task<string> AssessReadinessAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        await RunWithCaseContextAsync(caseId,
            "Conduct a rigorous pre-hearing readiness audit of this case. " +
            "You are a Senior Advocate doing a final review before a critical hearing. " +
            "You must identify EVERY gap, EVERY missing document, EVERY unprepared witness, " +
            "EVERY unfiled application, and EVERY procedural risk. " +
            "\n\n" +
            "AUDIT FRAMEWORK (score out of 100): " +
            "\n" +
            "A. EVIDENCE READINESS (30 points): " +
            "   - All key documents obtained and exhibited (10 pts) " +
            "   - Section 65B certificates obtained where needed (5 pts) " +
            "   - Expert witnesses briefed and affidavits filed (5 pts) " +
            "   - Documentary evidence certified and attested (5 pts) " +
            "   - Physical evidence properly preserved (5 pts) " +
            "\n" +
            "B. WITNESS READINESS (20 points): " +
            "   - All witnesses identified (5 pts) " +
            "   - Witness summons issued and served (5 pts) " +
            "   - Examination-in-chief questions prepared (5 pts) " +
            "   - Witnesses briefed and prepared (5 pts) " +
            "\n" +
            "C. LEGAL RESEARCH (20 points): " +
            "   - All applicable sections identified (5 pts) " +
            "   - Key judgments researched and printed (5 pts) " +
            "   - Written arguments/notes prepared (10 pts) " +
            "\n" +
            "D. PROCEDURAL COMPLIANCE (20 points): " +
            "   - All court fees paid (5 pts) " +
            "   - All orders complied with (5 pts) " +
            "   - All replies/written statements filed (5 pts) " +
            "   - All applications disposed/pending understood (5 pts) " +
            "\n" +
            "E. STRATEGY (10 points): " +
            "   - Clear objective for next hearing (5 pts) " +
            "   - Defence arguments anticipated and counters prepared (5 pts) " +
            "\n\n" +
            "Return ONLY this comprehensive JSON: " +
            "{" +
            "\"overallScore\": 72, " +
            "\"readinessVerdict\": \"READY FOR HEARING|PROCEED WITH CAUTION|DO NOT PROCEED — FILE ADJOURNMENT\", " +
            "\"dimensionScores\": {\"evidence\": 22, \"witnesses\": 14, \"research\": 15, \"procedural\": 16, \"strategy\": 5}, " +
            "\"strengths\": [{\"item\": \"Hospital records properly exhibited as Exhibit A\", \"points\": 5, \"dimension\": \"Evidence\"}], " +
            "\"criticalGaps\": [{\"gap\": \"Section 65B certificate not obtained for WhatsApp screenshot\", \"consequence\": \"WhatsApp exhibit may be rejected — destroys admission evidence\", \"remedy\": \"File application today for extension and obtain certificate from phone owner\", \"severity\": \"Critical\", \"daysToFix\": 2}], " +
            "\"highGaps\": [{\"gap\": \"Dr. Mehta not served with witness summons\", \"consequence\": \"Medical evidence cannot be proved without expert testimony\", \"remedy\": \"Issue summons immediately through court\", \"severity\": \"High\"}], " +
            "\"mediumGaps\": [{\"gap\": \"Written arguments not yet prepared\", \"remedy\": \"Prepare 10-page written argument noting — can be done in 1 day\", \"severity\": \"Medium\"}], " +
            "\"hearingRecommendation\": \"Proceed|Seek 2-week adjournment to fix critical gaps\", " +
            "\"adjournmentGrounds\": \"If adjournment needed — grounds to take: Petitioner's key witness Dr. Mehta unable to attend due to medical emergency at Apollo Hospital on the date fixed\", " +
            "\"prioritisedActionList\": [\"Action 1 to be done TODAY\", \"Action 2 by tomorrow\", \"Action 3 before hearing\"], " +
            "\"summary\": \"3-4 sentence professional readiness assessment\"" +
            "}",
            cancellationToken);

    // ============================================================
    // 12. EMERGENCY TRIAGE — Urgent legal response
    // ============================================================
    public Task<string> EmergencyTriageAsync(EmergencyRequestDto request, CancellationToken cancellationToken = default) =>
        aiClient.CompleteAsync(
            MasterIdentity +
            "\n\nEMERGENCY MODE ACTIVATED. An advocate needs immediate legal guidance. " +
            "Respond with the precision and speed of a Senior Advocate who has handled emergency matters " +
            "before Division Benches at 11 PM. " +
            "Be direct. Be specific. Be complete. Time is critical. " +
            "Respond in plain structured text (not JSON). Format: " +
            "SEVERITY: [Critical/High/Medium] " +
            "LEGAL ANALYSIS: [What is the exact legal position] " +
            "IMMEDIATE RISK: [What happens if no action taken in next 24 hours] " +
            "FIRST ACTION (Do this NOW): [Specific action within 1 hour] " +
            "SECOND ACTION (Do this within 24 hours): [Next action] " +
            "APPLICABLE SECTIONS: [Exact sections with act names] " +
            "DRAFT PRAYER (if urgent application needed): [Complete prayer clause] " +
            "RELEVANT JUDGMENTS: [1-2 directly applicable cases] " +
            "WHAT NOT TO DO: [Specific mistakes to avoid] " +
            "BEST CASE OUTCOME: [If handled correctly] " +
            "WORST CASE RISK: [If handled incorrectly]",
            "EMERGENCY QUERY:\n" + (request.Query ?? "No query provided"),
            cancellationToken);

    // ============================================================
    // 13. HEARING PREPARATION BRIEF — Complete day-of brief
    // ============================================================
    public Task<string> PrepHearingAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId,
            "Prepare a complete, court-ready hearing brief for the next hearing in this case. " +
            "This brief will be used by the advocate standing before the judge. " +
            "It must be comprehensive enough that the advocate can walk into court " +
            "having read only this brief and handle every possible situation. " +
            "\n\n" +
            "The brief must cover: " +
            "1. What happened last hearing and what is expected today " +
            "2. Exact opening submission (word-for-word) " +
            "3. All key arguments with judgment citations and paragraph numbers " +
            "4. What the opposing advocate will likely argue and exact counters " +
            "5. Every document to be tendered today with procedure " +
            "6. Every witness to be examined today with key questions " +
            "7. What orders to seek and exact prayer language " +
            "8. If adjournment becomes necessary — grounds and how to ask " +
            "\n\n" +
            "Return ONLY this comprehensive JSON: " +
            "{" +
            "\"hearingDate\": \"YYYY-MM-DD or Next Scheduled Date\", " +
            "\"hearingObjective\": \"The single most important thing to achieve today\", " +
            "\"contextSummary\": \"What happened last hearing and what stage we are at today\", " +
            "\"openingSubmission\": \"My Lord, this matter is listed today for [stage]. The Petitioner is represented by me. Briefly, this is a case of [one sentence summary]. Today we propose to [what you intend to do today].\", " +
            "\"mainArguments\": [{" +
            "\"argumentNumber\": 1, " +
            "\"heading\": \"On the issue of cruelty under Section 13(1)(ia) HMA\", " +
            "\"submission\": \"The complete argument to make — specific, detailed, with facts\", " +
            "\"supportingJudgments\": [\"Samar Ghosh v. Jaya Ghosh (2007) 4 SCC 511 — Para 101: Definition of cruelty\"], " +
            "\"supportingDocuments\": [\"Exhibit A — Hospital Record\", \"Exhibit B — Medical Certificate\"], " +
            "\"anticipatedInterruption\": \"Opposing counsel will say: These incidents are old and petitioner continued to live with respondent.\", " +
            "\"response\": \"My Lord, in V. Bhagat v. D. Bhagat (1994) 1 SCC 337, the Supreme Court held that isolated acts of cruelty over years cumulatively constitute grounds for divorce.\"" +
            "}], " +
            "\"documentsToTenderToday\": [{\"document\": \"Hospital Discharge Summary\", \"exhibitLabel\": \"Exhibit A\", \"procedure\": \"Tender original + 3 copies, examine Dr. Mehta to prove it\", \"anticipatedObjection\": \"Document not proved\", \"response\": \"We will call issuing authority\"}], " +
            "\"prayerForToday\": \"In view of the above, the Petitioner most humbly prays that this Hon'ble Court may be pleased to [specific prayer for today's hearing].\", " +
            "\"ordersToSeek\": [\"Direction to Respondent to file reply within 2 weeks\", \"Liberty to file rejoinder thereafter\"], " +
            "\"adjournmentStrategy\": {\"shouldWeSeek\": false, \"grounds\": \"If unavoidable — key witness Dr. Mehta is unavailable due to medical emergency\", \"howToAsk\": \"My Lord, I regret to inform the court that our key witness Dr. Mehta has suffered a medical emergency and is unable to attend today. We humbly seek 2 weeks to fix another date for his examination.\"}, " +
            "\"courtEtiquette\": \"Address court as: My Lord/Your Lordship (High Court/Supreme Court) | Your Honour (District/Family Court). Rise when judge enters/exits. Don't interrupt opposing counsel — object after they finish.\", " +
            "\"documentChecklist\": [\"Original + 3 copies of all exhibits\", \"Court fee receipts\", \"Vakalatnama\", \"Previous orders\", \"Note of arguments\"], " +
            "\"postHearingActions\": [\"File certified copy of today's order within 3 days\", \"Update client within 24 hours\", \"Complete any compliance ordered by next date\"]" +
            "}",
            cancellationToken);

    // ============================================================
    // 14. WITNESS PREPARATION — Complete examination strategy
    // ============================================================
    public Task<string> PrepWitnessAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        RunWithCaseContextAsync(caseId,
            "Prepare a comprehensive witness examination strategy for this case. " +
            "You are functioning as a trial advocacy specialist with 20 years of examination experience. " +
            "\n\n" +
            "LEGAL FRAMEWORK: " +
            "Order XVIII CPC for civil cases — examination of witnesses " +
            "Section 135-166 Indian Evidence Act — order of witnesses and examination " +
            "Section 137 — Examination-in-chief, cross-examination, re-examination " +
            "Section 138 — Order of examinations " +
            "Section 145 — Cross-examination on prior inconsistent statements " +
            "Section 146 — Questions lawful in cross-examination " +
            "Section 153 — Exclusion of evidence to contradict answers to questions testing veracity " +
            "\n\n" +
            "RULES OF EXAMINATION: " +
            "Leading questions NOT allowed in examination-in-chief (Section 141-143) " +
            "Leading questions ARE allowed in cross-examination " +
            "Re-examination limited to matters arising from cross-examination " +
            "Expert witnesses must have their qualifications established first " +
            "\n\n" +
            "Return ONLY this comprehensive JSON: " +
            "{" +
            "\"witnessOrder\": \"Recommended sequence of witnesses and strategic reason\", " +
            "\"ourWitnesses\": [{" +
            "\"serialNo\": 1, " +
            "\"name\": \"Petitioner — Priya Sharma\", " +
            "\"type\": \"Party Witness|Expert|Eyewitness|Character|Documentary\", " +
            "\"objective\": \"What this witness must establish through their testimony\", " +
            "\"credibilityScore\": 85, " +
            "\"credibilityBasis\": \"Why the judge will/will not believe this witness\", " +
            "\"examinationInChiefQuestions\": [" +
            "\"1. Please state your name, age, and address for the record.\", " +
            "\"2. When and where did your marriage with the Respondent take place?\", " +
            "\"3. Please describe in your own words what happened on the night of 12th August 2020.\"" +
            "], " +
            "\"keyDocumentsToProve\": [\"Hospital record Exhibit A — ask witness: Is this your discharge summary from Lilavati Hospital?\"], " +
            "\"anticipatedCrossExaminationAreas\": [\"Opposing counsel will challenge the delay between incident and complaint — prepare witness to explain\"], " +
            "\"preparationInstructions\": \"Tell witness: Answer only what is asked. Do not volunteer information. If you don't remember, say so. Do not get angry during cross-examination.\", " +
            "\"vulnerabilities\": [\"Delay of 2 months in filing complaint — must explain\"], " +
            "\"reExaminationPoints\": [\"If cross reveals new matters — clarify on these specific points\"]" +
            "}], " +
            "\"opposingWitnesses\": [{" +
            "\"serialNo\": 1, " +
            "\"name\": \"Respondent — Rohit Sharma\", " +
            "\"crossExaminationObjective\": \"Establish income concealment, destroy credibility, extract admissions\", " +
            "\"crossExaminationQuestions\": [" +
            "\"1. You have stated in your affidavit that your monthly income is Rs 22,000. Is that correct?\", " +
            "\"2. I am showing you a document — this is the RC Book of a BMW X5 registered in your name on 15th March 2024. Is this your vehicle?\", " +
            "\"3. The ex-showroom price of BMW X5 in 2024 was Rs 93 lakhs. How did you purchase this on Rs 22,000 per month income?\", " +
            "\"4. You stated you were never at home on the night of 12th August 2020. Is that your statement?\", " +
            "\"5. I am showing you Exhibit A — this is the hospital record of your wife showing admission on 12th August 2020 with injuries. You say you were not home that night?\"" +
            "], " +
            "\"admissionsToExtract\": [\"Get him to admit the BMW purchase\", \"Get him to admit his actual business income\", \"Get him to admit previous incidents of cruelty\"], " +
            "\"documentsToPutToCrossWitness\": [\"BMW RC Book\", \"Bank statements\", \"WhatsApp messages\"], " +
            "\"damageControl\": \"If witness becomes hostile — immediately move to next question, don't argue\"" +
            "}], " +
            "\"expertWitnessStrategy\": \"If medical expert called — establish: (1) qualifications (2) treatment of petitioner (3) nature of injuries consistent with assault (4) signed discharge summary is Exhibit A\", " +
            "\"generalStrategy\": \"Overall witness examination strategy for this case\"" +
            "}",
            cancellationToken);

    // ============================================================
    // 15. CASE CLASSIFIER
    // ============================================================
    public Task<string> ClassifyCaseTypeAsync(CaseTypeRequestDto request, CancellationToken cancellationToken = default) =>
        aiClient.CompleteAsync(
            MasterIdentity,
            "Classify this case based on the description. Be comprehensive and precise. " +
            "Return ONLY this JSON: " +
            "{" +
            "\"primaryCaseType\": \"Family|Criminal|Civil|Commercial|Tax|Labour|Constitutional|Consumer|Property|Intellectual Property\", " +
            "\"subType\": \"Divorce Petition|Maintenance|Custody|Cheque Bounce|Specific Performance|Injunction\", " +
            "\"confidence\": 94, " +
            "\"primarySections\": [{\"section\": \"Section 13(1)(ia) HMA 1955\", \"purpose\": \"Ground of cruelty — basis of divorce petition\"}], " +
            "\"additionalSections\": [{\"section\": \"Section 125 CrPC\", \"purpose\": \"Maintenance during pendency\"}], " +
            "\"recommendedCourt\": \"Family Court at [City]\", " +
            "\"jurisdictionBasis\": \"Section 10 Family Courts Act 1984 — exclusive jurisdiction\", " +
            "\"territorialJurisdiction\": \"File at place of last matrimonial home OR where petitioner resides\", " +
            "\"courtFeeEstimate\": \"Rs 200 for matrimonial petitions under Family Courts Act\", " +
            "\"documentationRequired\": [\"Marriage certificate\", \"Birth certificates of children\", \"Address proof\", \"Income documents\"], " +
            "\"limitationPeriod\": \"No limitation for divorce — but delay may be explained\", " +
            "\"estimatedDuration\": {\"familyCourt\": \"2-3 years\", \"highCourt\": \"4-6 years if appealed\", \"fastTrack\": \"1 year if uncontested\"}, " +
            "\"interimReliefsAvailable\": [\"Maintenance under Section 24 HMA\", \"Custody order under Section 26 HMA\", \"Injunction under Section 151 CPC\"], " +
            "\"similarLandmarkCases\": [{\"citation\": \"Samar Ghosh v. Jaya Ghosh (2007) 4 SCC 511\", \"relevance\": \"Defines legal cruelty comprehensively\"}]" +
            "} " +
            "\n\nCase description:\n" + (request.Description ?? ""),
            cancellationToken);

    // ============================================================
    // 16. DOCUMENT DRAFTING — Court-ready documents
    // ============================================================
    public Task<string> DraftDocumentAsync(Guid caseId, DraftRequestDto request, CancellationToken cancellationToken = default)
    {
        var draftType    = request.DraftType    ?? "Petition";
        var instructions = request.Instructions ?? "";

        var draftingPrompt =
            MasterIdentity +
            "\n\nYou are now functioning as India's finest drafting advocate with 30 years of experience " +
            "drafting documents for the Supreme Court and High Courts. " +
            "Your drafts are known for: " +
            "1. Zero errors in section citations and procedure " +
            "2. Logical, flowing paragraph structure that tells a compelling story " +
            "3. Comprehensive prayer clauses that leave nothing to chance " +
            "4. Language that commands judicial respect " +
            "5. Strategic framing that puts the case in the best possible light " +
            "6. Proper verification/affidavit formats as per court rules " +
            "\n\n" +
            "DRAFTING STANDARDS: " +
            "- Every paragraph must be numbered " +
            "- Every section cited must be quoted accurately " +
            "- Every case cited must include year and court " +
            "- Prayer must be comprehensive — include main relief, interim relief, and general relief " +
            "- Verification must follow correct format for the specific court " +
            "- If a petition — minimum 15-20 paragraphs for a complete petition " +
            "- If written arguments — minimum 8-10 pages " +
            "- If legal notice — professional letterhead format " +
            "\n\n" +
            GetDraftingSystemPrompt(draftType);

        return RunWithCaseContextAsync(caseId,
            "Draft a complete, court-ready " + draftType + " using all facts from the case dossier. " +
            "Special instructions: " + (string.IsNullOrEmpty(instructions) ? "None — prepare standard comprehensive draft" : instructions) + ". " +
            "\n\n" +
            "MANDATORY: " +
            "- Use actual names, dates, facts from the case dossier above " +
            "- Number every paragraph " +
            "- Include complete prayer clause " +
            "- Include proper verification " +
            "- Do NOT use placeholders like [Name] or [Date] — use actual data " +
            "- Return plain text formatted document (not JSON)",
            cancellationToken,
            draftingPrompt);
    }

    private static string GetDraftingSystemPrompt(string draftType) => draftType.ToLower() switch
    {
        var t when t.Contains("divorce") || t.Contains("petition") =>
            "DIVORCE PETITION FORMAT: " +
            "IN THE FAMILY COURT AT [CITY] " +
            "PETITION NO. ___ OF [YEAR] " +
            "IN THE MATTER OF: [Petitioner] v. [Respondent] " +
            "PETITION UNDER SECTION 13(1)(ia)/(ib) OF THE HINDU MARRIAGE ACT, 1955 " +
            "Minimum paragraphs: (1) Marriage details (2) Cohabitation (3) Children if any " +
            "(4-15) Specific incidents of cruelty with dates (16) Respondent's income and assets " +
            "(17) Inability to live together (18) Last date of cohabitation " +
            "PRAYER: (a) Decree of divorce (b) Permanent alimony (c) Custody of children (d) Litigation expenses (e) Any other relief " +
            "VERIFICATION: Sworn before Notary Public/Oath Commissioner",

        var t when t.Contains("maintenance") =>
            "MAINTENANCE APPLICATION FORMAT: " +
            "Under Section 125 CrPC or Section 24 HMA " +
            "Include: (1) Relationship (2) Inability to maintain self (3) Respondent's income in detail " +
            "(4) Petitioner's monthly expenses itemised (5) Children's expenses if applicable " +
            "PRAYER: (a) Maintenance from date of application (b) Legal expenses (c) Interim maintenance",

        var t when t.Contains("bail") =>
            "BAIL APPLICATION FORMAT: " +
            "Under Section 437/439 CrPC or Section 438 CrPC (anticipatory) " +
            "Include: (1) FIR details (2) Sections charged (3) Nature of alleged offence " +
            "(4) Applicant's background and roots in community (5) No flight risk " +
            "(6) Willing to cooperate with investigation (7) Similar cases where bail granted " +
            "CONDITIONS OFFERED: Surety, passport surrender, weekly attendance, etc.",

        var t when t.Contains("injunction") =>
            "INJUNCTION APPLICATION FORMAT: " +
            "Under Order XXXIX Rules 1 & 2 CPC read with Section 151 CPC " +
            "Three essentials to establish: " +
            "(1) PRIMA FACIE CASE — strong case on merits " +
            "(2) BALANCE OF CONVENIENCE — hardship if injunction not granted " +
            "(3) IRREPARABLE INJURY — damage that cannot be compensated in money " +
            "UNDERTAKING: Plaintiff undertakes to pay damages if injunction wrongly granted",

        var t when t.Contains("written statement") =>
            "WRITTEN STATEMENT FORMAT: " +
            "Under Order VIII CPC " +
            "Must contain: (1) Preliminary objections (2) Denial of each allegation para-by-para " +
            "(3) Positive defence (4) Set-off if applicable " +
            "Rule: Every allegation not specifically denied is deemed admitted " +
            "Limitation: Must be filed within 30 days (extendable to 90 days max under Order VIII Rule 1)",

        var t when t.Contains("legal notice") =>
            "LEGAL NOTICE FORMAT: " +
            "On advocate's letterhead " +
            "By RPAD/Email with acknowledgment " +
            "Include: (1) Full name and address of sender (2) Full name and address of recipient " +
            "(3) Clear statement of facts (4) Legal basis for demand (5) Specific demand " +
            "(6) Time limit — 15 or 30 days (7) Consequence of non-compliance " +
            "Close: Sent without prejudice to all rights",

        _ =>
            "Follow standard Indian court format for " + draftType + ". " +
            "Number all paragraphs. Include complete prayer. Include verification."
    };

    // ============================================================
    // HELPER — Run with full case context
    // ============================================================
    private async Task<string> RunWithCaseContextAsync(
        Guid caseId,
        string instruction,
        CancellationToken cancellationToken,
        string? customSystemPrompt = null)
    {
        var dossier      = await BuildCaseDossierAsync(caseId, cancellationToken);
        var systemPrompt = (customSystemPrompt ?? MasterIdentity) + "\n\n" + dossier;
        return await aiClient.CompleteAsync(systemPrompt, instruction, cancellationToken);
    }

    // ============================================================
    // HELPER — Build exhaustive case dossier
    // ============================================================
    private async Task<string> BuildCaseDossierAsync(Guid caseId, CancellationToken cancellationToken)
    {
        var c = await db.Cases
            .Include(c => c.Client)
            .Include(c => c.Hearings).ThenInclude(h => h.Orders)
            .Include(c => c.TimelineEvents)
            .Include(c => c.Contradictions)
            .Include(c => c.LegalResearches)
            .Include(c => c.ActionPlans)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(x => x.Id == caseId, cancellationToken)
            ?? throw new InvalidOperationException("Case not found.");

        var sb = new StringBuilder();
        sb.AppendLine("══════════════════════════════════════════════════════════");
        sb.AppendLine("CLAUSIO — COMPLETE CASE DOSSIER");
        sb.AppendLine("══════════════════════════════════════════════════════════");

        sb.AppendLine("\n■ CASE IDENTIFICATION");
        sb.AppendLine("Name:            " + c.Name);
        sb.AppendLine("Case Number:     " + c.CaseNumber);
        sb.AppendLine("Type:            " + c.CaseType + " / " + c.SubType);
        sb.AppendLine("Court:           " + c.Court + " at " + c.CourtLocation);
        sb.AppendLine("Stage:           " + c.Stage);
        sb.AppendLine("Status:          " + c.Status + " | Priority: " + c.Priority);
        sb.AppendLine("Filed On:        " + c.FiledOn.ToString("dd MMMM yyyy"));
        sb.AppendLine("Next Hearing:    " + (c.NextHearing?.ToString("dd MMMM yyyy") ?? "Not yet scheduled"));
        sb.AppendLine("Opposing Adv:    " + (c.OpposingAdv ?? "Name not recorded"));

        sb.AppendLine("\n■ CLIENT PROFILE");
        if (c.Client != null)
        {
            sb.AppendLine("Full Name:       " + c.Client.FirstName + " " + c.Client.LastName);
            sb.AppendLine("Phone:           " + (c.Client.Phone ?? "Not recorded"));
            sb.AppendLine("Occupation:      " + (c.Client.Occupation ?? "Not recorded"));
            sb.AppendLine("Monthly Income:  " + (c.Client.MonthlyIncome.HasValue ? "Rs " + c.Client.MonthlyIncome.Value.ToString("N0") + "/month" : "Not recorded"));
            sb.AppendLine("Address:         " + (c.Client.Address ?? "Not recorded"));
        }

        if (c.TimelineEvents.Count > 0)
        {
            sb.AppendLine("\n■ CASE TIMELINE (" + c.TimelineEvents.Count + " events)");
            foreach (var e in c.TimelineEvents.OrderBy(x => x.SortOrder))
            {
                sb.AppendLine("  [" + e.EventDate.ToString("dd MMM yyyy") + "] [" + e.Category + "] " + e.Event);
                if (!string.IsNullOrEmpty(e.LegalSignificance))
                    sb.AppendLine("    → Legal significance: " + e.LegalSignificance);
                if (!string.IsNullOrEmpty(e.Source))
                    sb.AppendLine("    → Source/Proof: " + e.Source);
            }
        }

        if (c.Documents.Count > 0)
        {
            sb.AppendLine("\n■ DOCUMENTS ON RECORD (" + c.Documents.Count + " documents)");
            foreach (var d in c.Documents)
            {
                sb.AppendLine("  • " + d.FileName);
                sb.AppendLine("    Type: " + d.DocumentType +
                             " | Exhibit: " + (d.ExhibitLabel ?? "Not yet labelled") +
                             " | Size: " + (d.FileSize > 0 ? Math.Round(d.FileSize / 1024.0, 1) + " KB" : "Unknown"));
                if (!string.IsNullOrEmpty(d.ExtractedText))
                    sb.AppendLine("    [AI-analysed — key content available]");
            }
        }
        else
        {
            sb.AppendLine("\n■ DOCUMENTS: None uploaded yet — advocate must gather documentary evidence");
        }

        if (c.Hearings.Count > 0)
        {
            sb.AppendLine("\n■ HEARING HISTORY (" + c.Hearings.Count + " hearings)");
            foreach (var h in c.Hearings.OrderByDescending(x => x.HearingDate))
            {
                sb.AppendLine("  [" + h.HearingDate.ToString("dd MMM yyyy") + "] Stage: " + h.Stage);
                sb.AppendLine("    What happened: " + h.WhatHappened);
                if (!string.IsNullOrEmpty(h.JudgeObservation))
                    sb.AppendLine("    Judge said: " + h.JudgeObservation);
                if (!string.IsNullOrEmpty(h.NextObjective))
                    sb.AppendLine("    Next objective: " + h.NextObjective);
                foreach (var o in h.Orders)
                {
                    sb.AppendLine("    ORDER: " + o.Text);
                    sb.AppendLine("    Due: " + (o.Deadline?.ToString("dd MMM yyyy") ?? "No deadline") +
                                 " | Status: " + (o.Done ? "✓ COMPLIED" : "⚠ PENDING COMPLIANCE"));
                }
            }
        }
        else
        {
            sb.AppendLine("\n■ HEARINGS: No hearings recorded yet");
        }

        if (c.Contradictions.Count > 0)
        {
            sb.AppendLine("\n■ CONTRADICTIONS IDENTIFIED (" + c.Contradictions.Count + " contradictions)");
            foreach (var x in c.Contradictions)
            {
                sb.AppendLine("  CLAIM: " + x.Claim);
                sb.AppendLine("  Source: " + x.ClaimSource);
                sb.AppendLine("  EVIDENCE AGAINST: " + x.Evidence);
                sb.AppendLine("  Source: " + x.EvidenceSource);
                sb.AppendLine("  Strength: " + x.Strength);
                sb.AppendLine("  Court use: " + x.CourtArgument);
                sb.AppendLine();
            }
        }

        if (c.LegalResearches.Count > 0)
        {
            sb.AppendLine("\n■ LEGAL RESEARCH (" + c.LegalResearches.Count + " judgments)");
            foreach (var r in c.LegalResearches)
            {
                sb.AppendLine("  • " + r.Citation + " (" + r.Court + ", " + r.Year + ")");
                sb.AppendLine("    Ratio: " + r.RatioDecidendi);
                sb.AppendLine("    Relevance: " + r.Relevance);
                sb.AppendLine("    Use: " + r.HowToUse);
                sb.AppendLine("    Strength: " + r.Strength);
            }
        }

        if (c.ActionPlans.Count > 0)
        {
            sb.AppendLine("\n■ ACTION PLAN STATUS (" + c.ActionPlans.Count + " items)");
            var pending  = c.ActionPlans.Where(a => !a.Done).OrderBy(a => a.DueBy).ToList();
            var done     = c.ActionPlans.Where(a => a.Done).ToList();
            if (pending.Any())
            {
                sb.AppendLine("  PENDING:");
                foreach (var a in pending)
                    sb.AppendLine("  ○ [" + a.Priority + "] " + a.Title + " — Due: " + a.DueBy.ToString("dd MMM yyyy") + " — " + a.AssignedTo);
            }
            if (done.Any())
            {
                sb.AppendLine("  COMPLETED:");
                foreach (var a in done)
                    sb.AppendLine("  ✓ " + a.Title);
            }
        }

        sb.AppendLine("\n══════════════════════════════════════════════════════════");
        sb.AppendLine("END OF CASE DOSSIER");
        sb.AppendLine("══════════════════════════════════════════════════════════");

        return sb.ToString();
    }
}
