# Skarabeus
## funkční požadavky
 - #### registrace a přihlášení uživatelů
   - správa oprávnění uživatelů
      - změna oprávnění => přidání, odebrání
      - vytvuření uživatelského přihlášení pro osobu
 - #### vytváření a správa osob 
   - základní informace (jméno, příjmení ...)
   - kontaktní údaje
   - informace o zákoných zástupcích
 - #### vytváření a správa akcí
   - datum trvání
   - jídla
   - účastníci
 - #### vytváření a správa jídel
   - název
   - ingredience
   - popis
 - #### vytváření a správa ingrediencí
   - název
   - přibližnou cenu za kg/l

## nefunkční požadavky
 - #### Bezpečnost:
   - Ověřování uživatelů při registraci a přihlašování.
   - Ochrana osobních údajů uživatelů (email, hash hesla).
 - #### Výkon:
   - Všechny akce v aplikaci musí být prováděny v reálném čase do 0,5 s (přihlášení, kladení dotazů, aktualizace statusů).
   - Aplikace musí být schopna zvládnout alespoň 200 připojených uživatelů, včetně správy skupin a místností.
 - #### Dostupnost:
   - Aplikace by měla být přístupná 24/7 s minimálními výpadky v rámci dostupných nástrojů, které jsou zdarma.
 - #### Responzivita:
   - Zobrazení aplikace by mělo být optimalizované pro různé druhy zařízení.
