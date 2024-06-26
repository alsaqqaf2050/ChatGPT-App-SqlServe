﻿USE [Responses]
GO
/****** Object:  StoredProcedure [dbo].[SearchResponses]    Script Date: 6/2/2024 2:13:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SearchResponses]
    @Prompt NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    -- تقسيم السؤال إلى كلمات فردية
    DECLARE @WordsTable TABLE (Word NVARCHAR(100));
    INSERT INTO @WordsTable (Word)
    SELECT value FROM STRING_SPLIT(@Prompt, ' ');

    -- جدول مؤقت لتخزين النتائج المؤقتة
    DECLARE @Results TABLE (Response NVARCHAR(MAX), MatchCount INT);

    -- البحث باستخدام LIKE
    INSERT INTO @Results (Response, MatchCount)
    SELECT R.Response, COUNT(*)
    FROM Responses R
    JOIN @WordsTable WT ON R.Response LIKE '%' + WT.Word + '%'
    GROUP BY R.Response;

    -- البحث باستخدام LIKE مع الفحص الكامل على النص
    INSERT INTO @Results (Response, MatchCount)
    SELECT R.Response, (SELECT COUNT(*) FROM @WordsTable)
    FROM Responses R
    WHERE EXISTS (
        SELECT 1
        FROM @WordsTable W
        WHERE R.Response LIKE '%' + W.Word + '%'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM @WordsTable W
        WHERE R.Response NOT LIKE '%' + W.Word + '%'
    );

    -- اختيار أعلى 5 ردود متناسبة
    SELECT TOP 5 Response
    FROM @Results
    ORDER BY MatchCount DESC;
END
