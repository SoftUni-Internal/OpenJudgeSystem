var token = process.env.GITHUB_TOKEN;
var owner = process.env.ORGANIZATION_NAME;
var repositoryName = process.env.REPO_NAME;
var prNumber = parseInt(process.env.PULL_REQUEST_NUMBER);

var returnedForChangesMilestoneName = '2. Returned for changes';

async function main() {
  var result = await getPrLinkedIssues(repositoryName, owner, prNumber);
  var resultIssues = result.data?.repository?.pullRequest?.closingIssuesReferences?.nodes;
  
  if (resultIssues !== undefined && resultIssues.length > 0) {
    resultIssues.forEach(async issue => {

      var issueRepo = issue?.repository?.name;
      
      let milestonesResult = await getIssueRepoMilestones(owner, issueRepo);
      let milestonesData = milestonesResult?.data?.repository?.milestones?.nodes;
      let milestoneId = milestonesData?.find(obj => obj.title === returnedForChangesMilestoneName)?.id;
      
      if(milestoneId !== undefined){
        updateIssueMilestone(issue['id'], milestoneId);
      }
    });
  }
}

async function getIssueRepoMilestones(owner, repo) {
  const result = await fetchGitHubAPI(
    `query ($repo: String!, $owner: String!) {
      repository(name: $repo, owner: $owner) {
        milestones(states: OPEN, first:20){
          nodes{
            id, title
          }
        }
      }
    }`,
    { repo, owner }
  );

  return result;
}

async function updateIssueMilestone(issueId, milestoneId) {
  const result = await fetchGitHubAPI(
    `mutation updateIssueMilestone($issueId:ID!, $milestoneId:ID!) {
        updateIssue(input: {id: $issueId, milestoneId: $milestoneId}){
          clientMutationId
        }
      }`,
    { issueId, milestoneId }
  );
}

async function getPrLinkedIssues(repositoryName, owner, prNumber) {
  const result = await fetchGitHubAPI(
    `query($repo: String!, $owner:String!, $prNumber: Int!){
        repository(name: $repo, owner: $owner) {
          pullRequest(number: $prNumber){
            closingIssuesReferences(first:10){
              nodes{
                id, title, repository {
                  name
                }
              }
            }
        }
    }
}`,
    { repo: repositoryName, owner, prNumber }
  );
  return result;
}


async function fetchGitHubAPI(query, variables) {
  const result = await fetch('https://api.github.com/graphql', {
    method: 'POST',
    headers: {
      Authorization: `bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      query,
      variables,
    })
  });

  return await result.json();
}

main();