SELECT     ProjectUID, ProjectName, ProjectStartDate, ProjectFinishDate, '0' as [Type]
into #t1
FROM         MSP_EpmProject_UserView

SELECT     ProjectUID, CIMBTaskType AS Title, MIN(TaskStartDate) AS Start, MAX(TaskFinishDate) AS [End], '1' as [Type]
into #t2
FROM         MSP_EpmTask_UserView
GROUP BY CIMBTaskType, ProjectUID
HAVING      (CIMBTaskType IS NOT NULL)

INSERT into #t2
select #t1.ProjectUID, #t1.ProjectName, #t1.ProjectStartDate, #t1.ProjectFinishDate, #t1.[Type]
FROM #t1 INNER JOIN (SELECT DISTINCT #t2.ProjectUID from #t2) AS t2temp ON t2temp.ProjectUID =#t1.ProjectUID

SELECT     *
FROM         [#t2]
ORDER BY ProjectUID,[Type], Start

drop table #t1
drop table #t2