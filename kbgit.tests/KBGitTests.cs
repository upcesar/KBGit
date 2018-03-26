﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KbgSoft.KBGit;
using Xunit;

namespace kbgit.tests
{
    public class KbGitTests
    {
	    private RepoBuilder repoBuilder = new RepoBuilder();

		[Fact]
	    public void When_committing_Then_move_branch_pointer()
	    {
		    var id1 = repoBuilder.EmptyRepo().AddFile("a.txt").Commit();

		    var branchTip = repoBuilder.Git.Hd.Branches.Single().Value.Tip;
		    Assert.Equal(id1, branchTip);
	    }

	    [Fact]
	    public void When_committing_Then_set_parent_to_previous_head()
	    {
			var parentId = repoBuilder.EmptyRepo().AddFile("a.txt").Commit();

		    var commitId = repoBuilder.AddFile("a.txt").Commit();

		    Assert.Equal(parentId, repoBuilder.Git.Hd.Commits[commitId].Parents.First());
	    }

		[Fact]
	    public void CommitWhenHeadless()
	    {
		    repoBuilder = new RepoBuilder(@"c:\temp\");
		    var git = repoBuilder.Build2Files3Commits();
			git.Checkout(git.HeadRef(1));
		    repoBuilder.AddFile("newfile", "dslfk");

		    var id = git.Commit("headless commit", "a", new DateTime(2010, 11, 12), git.ScanFileSystem());

			Assert.Equal("2ef282b36b6b71420c33b234ca74c4f185dc88948f6cbfa6164d01c5b2993448", id.ToString());
	    }

	    [Fact]
	    public void When_Commit_a_similar_situation_to_a_previous_commit_Then_should_not_incread_blobs_nor_tree_blobs()
	    {
		    repoBuilder
			    .EmptyRepo()
			    .AddFile("a.txt")
			    .Commit();

		    repoBuilder
			    .AddFile("b.txt")
			    .Commit();

		    Assert.Equal(2, repoBuilder.Git.Hd.Blobs.Count);
		    Assert.Equal(2, repoBuilder.Git.Hd.Commits.Count);

			repoBuilder
			    .DeleteFile("b.txt")
			    .Commit("deleted b");

		    Assert.Equal(2, repoBuilder.Git.Hd.Blobs.Count);
		    Assert.Equal(3, repoBuilder.Git.Hd.Commits.Count);
		}

		public string FileSystemScanFolder(KBGit git)
		{
			return git.FileSystemScanFolder(git.CodeFolder).ToString();
		}

		[Fact]
	    public void Given_two_toplevel_files_Then_()
	    {
		    repoBuilder = new RepoBuilder(@"c:\temp\");
		    var git = repoBuilder.BuildEmptyRepo();
		    repoBuilder.AddFile("car.txt", "car");
		    repoBuilder.AddFile("door.txt", "door");

			var files = FileSystemScanFolder(git);

		    Assert.Equal(@"tree 2 
blob car.txt
blob door.txt",  files);
	    }

		[Fact]
		public void Given_two_files_in_subfolder_Then_()
	    {
		    repoBuilder = new RepoBuilder(@"c:\temp\");
		    var git = repoBuilder.BuildEmptyRepo();
		    repoBuilder.AddFile(@"FeatureVolvo\car.txt", "car");
		    repoBuilder.AddFile(@"FeatureVolvo\door.txt", "door");

			var files = FileSystemScanFolder(git);

		    Assert.Equal(
@"tree 1 
tree 2 FeatureVolvo
blob FeatureVolvo\car.txt
blob FeatureVolvo\door.txt", files);
		}

		[Fact]
		public void Get_folders_and_files()
	    {
		    repoBuilder = new RepoBuilder(@"c:\temp\");
		    var git = repoBuilder.BuildEmptyRepo();
		    repoBuilder.AddFile(@"FeatureVolvo\car.txt", "car");
		    repoBuilder.AddFile(@"FeatureGarden\tree.txt", "tree");
		    repoBuilder.AddFile(@"FeatureGarden\shovel.txt", "shovel");
		    repoBuilder.AddFile(@"FeatureGarden\Suburb\grass.txt", "grass");
		    repoBuilder.AddFile(@"FeatureGarden\Suburb\mover.txt", "mover");

			var files = FileSystemScanFolder(git);

		    Assert.Equal(
@"tree 2 
tree 3 FeatureGarden
blob FeatureGarden\shovel.txt
blob FeatureGarden\tree.txt
tree 2 FeatureGarden\Suburb
blob FeatureGarden\Suburb\grass.txt
blob FeatureGarden\Suburb\mover.txt
tree 1 FeatureVolvo
blob FeatureVolvo\car.txt"
			    , files);
	    }

	    [Fact]
	    public void Visit()
	    {
		    var repoBuilder = new RepoBuilder(@"c:\temp\");
		    var git = repoBuilder.BuildEmptyRepo();
		    repoBuilder.AddFile(@"FeatureVolvo\car.txt", "car");
		    repoBuilder.AddFile(@"FeatureGarden\tree.txt", "tree");
		    repoBuilder.AddFile(@"FeatureGarden\shovel.txt", "shovel");
		    repoBuilder.AddFile(@"FeatureGarden\Suburb\grass.txt", "grass");
		    string buf = "";
			git.FileSystemScanFolder(git.CodeFolder).Visit(x =>
			{
				if (x is TreeTreeLine t)
					buf += $"visittree {t.Path}\r\n";
				if (x is BlobTreeLine b)
					buf += $"visitblob {b.Path}\r\n";
			});

		    Assert.Equal(@"visittree 
visittree FeatureGarden
visitblob FeatureGarden\shovel.txt
visitblob FeatureGarden\tree.txt
visittree FeatureGarden\Suburb
visitblob FeatureGarden\Suburb\grass.txt
visittree FeatureVolvo
visitblob FeatureVolvo\car.txt
", buf);
	    }
	}

	public class KBGitHeadTests
	{
		RepoBuilder repoBuilder = new RepoBuilder();

		[Fact]
		public void Given_fresh_repo_When_getting_headinfo_Then_fail()
		{
			var git = new RepoBuilder(@"c:\temp\").BuildEmptyRepo();

			Assert.Null(git.Hd.Head.GetId(git.Hd));
			Assert.Equal("master", git.Hd.Head.Branch);
		}

		[Fact]
		public void Given_repo_When_committing_getting_headinfo_Then_return_info()
		{
			repoBuilder = new RepoBuilder(@"c:\temp\");
			var firstId = repoBuilder.EmptyRepo().AddFile("a.txt").Commit();

			Assert.Equal(firstId, repoBuilder.Git.Hd.Head.GetId(repoBuilder.Git.Hd));
			Assert.Null(repoBuilder.Git.Hd.Head.Id);
			Assert.Equal("master", repoBuilder.Git.Hd.Head.Branch);
		}

		[Fact]
		public void Given_repo_When_getting_HeadRef_1_Then_return_parent_of_HEAD()
		{
			var git = repoBuilder.Build2Files3Commits();
			var parentOfHead = git.Hd.Commits[git.Hd.Head.GetId(git.Hd)].Parents.First();

			Assert.Equal(parentOfHead, git.HeadRef(1));
		}

		[Fact]
		public void When_detached_head_and_commit_move_Then_update_head()
		{
			var detachedId = repoBuilder
				.EmptyRepo()
				.AddFile("a.txt")
				.Commit();
			repoBuilder
				.AddFile("b.txt")
				.Commit();
			repoBuilder.Git.Checkout(detachedId);
			var detachedId2=repoBuilder
				.AddFile("a.txt")
				.Commit();

			Assert.Null(repoBuilder.Git.Hd.Head.Branch);
			Assert.Equal(detachedId2, repoBuilder.Git.Hd.Head.Id);
			Assert.Equal(detachedId2, repoBuilder.Git.Hd.Head.GetId(repoBuilder.Git.Hd));
		}
	}

	public class IdTests
	{
		[Fact]
		public void When_equal_Then_equals_returns_true()
		{
			var id1 = new Id("162b60c2809016e893b96d2d941c0c68ba6d2ac25cbc12b21d5678656fca8c8f");
			var id2 = new Id("162b60c2809016e893b96d2d941c0c68ba6d2ac25cbc12b21d5678656fca8c8f");

			Assert.True(id1.Equals(id2));
		}

		[Fact]
		public void When_not_equal_Then_equals_returns_false()
		{
			var id1 = new Id("162b60c2809016e893b96d2d941c0c68ba6d2ac25cbc12b21d5678656fca8c8f");
			var id2 = new Id("612b60c2809016e893b96d2d941c0c68ba6d2ac25cbc12b21d5678656fca8c8f");

			Assert.False(id1.Equals(id2));
		}
	}

	public class LogTests
	{


		[Fact]
		public void When_empty_repo_Then_log_returns_empty()
		{
			var repoBuilder = new RepoBuilder().EmptyRepo();

			Assert.Equal(@"Log for master
", repoBuilder.Git.Log());
		}

		[Fact]
		public void When_one_commit_Then_log_one_line()
		{
			var repoBuilder = new RepoBuilder(new Guid("6c7f821e-5cb2-45de-9365-3e35887c0ee6"))
				.EmptyRepo()
				.AddFile("a.txt", "some content");

			repoBuilder.Git.Commit("Add a.txt", "kasper graversen", new DateTime(2018, 3, 1, 12, 22, 33));

			Assert.Equal(@"Log for master
* 06cd57d - Add a.txt (2018/03/01 12:22:33) <kasper graversen> 
", repoBuilder.Git.Log());
		}

		[Fact]
		public void When_two_commits_Then_log_twoline()
		{
			var repoBuilder = new RepoBuilder(new Guid("b3b12f1c-f455-4987-b2d7-5db08d9e1ee4"))
				.EmptyRepo()
				.AddFile("a.txt", "some content");
			repoBuilder.Git.Commit("Add a.txt", "kasper graversen", new DateTime(2018, 3, 1, 12, 22, 33));
			repoBuilder.AddFile("a.txt", "changed a...");
			repoBuilder.Git.Commit("Changed a.txt", "kasper graversen", new DateTime(2018, 3, 2, 13, 24, 34));

			Assert.Equal(@"Log for master
* dd30447 - Changed a.txt (2018/03/02 01:24:34) <kasper graversen> 
* 06cd57d - Add a.txt (2018/03/01 12:22:33) <kasper graversen> 
", repoBuilder.Git.Log());
		}

		[Fact]
		public void When_two_commits_on_master_and_one_on_feature_Then_log_both_branches()
		{
			var repoBuilder = new RepoBuilder(new Guid("186d2ac8-1e9c-4e86-b1ac-b18208adead4"))
				.EmptyRepo()
				.AddFile("a.txt", "some content");
			repoBuilder.Git.Commit("Add a.txt", "kasper graversen", new DateTime(2018, 3, 1, 12, 22, 33));
			repoBuilder.AddFile("a.txt", "changed a...");
			repoBuilder.Git.Commit("Changed a.txt", "kasper graversen", new DateTime(2018, 3, 2, 13, 24, 34));
			repoBuilder
				.NewBranch("feature/speed")
				.AddFile("a.txt", "speedup!")
				.Git.Commit("Speedup a.txt", "kasper graversen", new DateTime(2018, 4, 3, 15, 26, 37));

			Assert.Equal(@"Log for master
* dd30447 - Changed a.txt (2018/03/02 01:24:34) <kasper graversen> 
* 06cd57d - Add a.txt (2018/03/01 12:22:33) <kasper graversen> 
Log for feature/speed
* fafcd20 - Speedup a.txt (2018/04/03 03:26:37) <kasper graversen> 
* dd30447 - Changed a.txt (2018/03/02 01:24:34) <kasper graversen> 
* 06cd57d - Add a.txt (2018/03/01 12:22:33) <kasper graversen> 
", repoBuilder.Git.Log());
		}

	}

	public class GitCommitTests
	{
		RepoBuilder repoBuilder = new RepoBuilder();

		[Fact]
		public void When_Commit_Then_treeid_information_is_Stored()
		{
			repoBuilder.EmptyRepo().AddFile("a.txt").Commit();

			var commit = repoBuilder.Git.Hd.Commits.First();
			Assert.NotNull(commit.Value.Tree);
			Assert.NotNull(commit.Value.TreeId);
		}

		[Fact]
		public void When_Commit_Then_content_is_stored()
		{
			var filename = "a.txt";
			var id1 = repoBuilder
				.EmptyRepo()
				.AddFile(filename, "version 1 a")
				.Commit();
			var id2 = repoBuilder
				.AddFile(filename, "version 2 a")
				.Commit();

			repoBuilder.Git.Checkout(id1);
			Assert.Equal("version 1 a", repoBuilder.ReadFile(filename));

			repoBuilder.Git.Checkout(id2);
			Assert.Equal("version 2 a", repoBuilder.ReadFile(filename));
		}

		[Fact]
		public void When_branching_and_commit_and_update_back_Then_reset_content_to_old_branch()
		{
			repoBuilder
				.EmptyRepo()
				.AddFile("a.txt", "version 1 a")
				.Commit();
			repoBuilder.NewBranch("featurebranch")
				.AddFile("b.txt", "version 1 b")
				.Commit();
			IEnumerable<string> FilesInRepo() => repoBuilder.Git.ScanFileSystem().Select(x => x.Path);

			Assert.Equal(new[] {"a.txt", "b.txt"}, FilesInRepo());

			repoBuilder.Git.Checkout("master");
			Assert.Equal(new[] { "a.txt" }, FilesInRepo());

			repoBuilder.Git.Checkout("featurebranch");
			Assert.Equal(new[] { "a.txt", "b.txt" }, FilesInRepo());
		}

		[Fact]
		public void When_detached_head_Then_git_branches_shows_detached_as_branch()
		{
			repoBuilder = new RepoBuilder(@"c:\temp\");
			var detachedId = repoBuilder
				.EmptyRepo()
				.AddFile("a.txt")
				.Commit();
			repoBuilder
				.AddFile("b.txt")
				.Commit();

			repoBuilder.Git.Checkout(detachedId);

			Assert.Equal($@"* (HEAD detached at {detachedId.ToString().Substring(0, 7)})
  master", repoBuilder.Git.Branch());
		}

		[Fact]
		public void When_branching_Then_Branchinfo_show_new_branchname()
		{
			repoBuilder = new RepoBuilder(@"c:\temp\");
			repoBuilder
				.EmptyRepo()
				.AddFile("a.txt")
				.Commit();
			Assert.Equal("* master", repoBuilder.Git.Branch());

			repoBuilder.NewBranch("featurebranch");
			Assert.Equal(@"* featurebranch
  master", repoBuilder.Git.Branch());
		}
	}

	public class NetworkingTests
	{
		[Fact]
		public void When_pulling_Then_receive_all_nodes()
		{
			var remoteGit = new RepoBuilder().Build2Files3Commits();
			var gitServer = SpinUpServer(remoteGit, 18081);
			var localGit = new RepoBuilder().EmptyRepo().AddLocalHostRemote(18081).Git;

			new GitNetworkClient().PullBranch(localGit.Hd.Remotes.First(), "master", localGit);

			Assert.Equal(@"Log for master
Log for origin/master
* d2c19da - Add a2 (2017/03/03 03:03:03) <kasper> 
* 5b65531 - Add b (2017/02/02 02:02:02) <kasper> 
* 27047ec - Add a (2017/01/01 01:01:01) <kasper> 
", localGit.Log());
			gitServer.Abort();
		}

		[Fact]
		public void When_pushing_Then_push_nodes_and_update_branchpointer_on_server()
		{
			var remoteGit = new RepoBuilder().BuildEmptyRepo();
			var gitServer = SpinUpServer(remoteGit, 18083);
			var localbuilder = new RepoBuilder();
			var localGit = localbuilder.Build2Files3Commits();
			localbuilder.AddLocalHostRemote(18083);

			Branch branch = localGit.Hd.Branches["master"];
			var commits = localGit.GetReachableNodes(branch.Tip).ToArray();
			new GitNetworkClient().PushBranch(localGit.Hd.Remotes.First(), "master", branch, null, commits);

			Assert.Equal(@"Log for master
* d2c19da - Add a2 (2017/03/03 03:03:03) <kasper> 
* 5b65531 - Add b (2017/02/02 02:02:02) <kasper> 
* 27047ec - Add a (2017/01/01 01:01:01) <kasper> 
", remoteGit.Log());
			gitServer.Abort();
		}

		private Task t;
		GitServer SpinUpServer(KBGit git, int port)
		{
			var server = new GitServer(git);
			t = new TaskFactory().StartNew(() => server.StartDaemon(port));

			while (!server.Running.HasValue)
				Thread.Sleep(50);

			return server;
		}
	}

	public class CommandHandlingTests
	{
		bool logWasMatched = false;
		bool commitWasMatched = false;
		readonly RepoBuilder repoBuilder = new RepoBuilder();

		(string, string[], Action<KBGit, string[]>)[] GetTestConfigurationStub()
		{
			(string, string[], Action<KBGit, string[]>)[] configuration =
			{
				("make log", new[] {"log"}, (git, args) => { logWasMatched = true; }),
				("make commit", new[] {"commit", "<message>"}, (git, args) => { commitWasMatched = true; }),
			};

			return configuration;
		}

		[Fact]
		public void When_printhelp_Then_all_commands_are_explained()
		{
			var helpText = new CommandlineHandling().CreateHelpText(CommandlineHandling.MakeConfig());
			Assert.Equal(
@"KBGit Help
----------
git init                               - Initialize an empty repo
git commit -m <message>                - Make a commit
git log                                - Show the commit log
git checkout -b <branchname>           - Create a new new branch at HEAD
git checkout -b <branchname> <id>      - Create a new new branch at commit id
git checkout <id>                      - Update HEAD
git branch -D <branchname>             - Delete a branch
git branch                             - List existing branches
git gc                                 - Garbage collect", helpText);
		}

		[Fact]
		public void When_calling_with_specific_arguments_Then_match()
		{
			new CommandlineHandling().Handle(repoBuilder.EmptyRepo().Git, GetTestConfigurationStub(), new[] { "log" });

			Assert.True(logWasMatched);
			Assert.False(commitWasMatched);
		}

		[Fact]
		public void When_not_calling_with_unrecognized_arguments_Then_not_match()
		{
			new CommandlineHandling().Handle(repoBuilder.EmptyRepo().Git, GetTestConfigurationStub(), new[] {"NOTLOG"});

			Assert.False(logWasMatched);
			Assert.False(commitWasMatched);
		}

		[Fact]
		public void When_calling_with_too_few_arguments_Then_not_match()
		{
			new CommandlineHandling().Handle(repoBuilder.EmptyRepo().Git, GetTestConfigurationStub(), new[] { "git" });

			Assert.False(logWasMatched);
			Assert.False(commitWasMatched);
		}

		[Fact]
		public void When_calling_with_too_many_arguments_Then_not_match()
		{
			new CommandlineHandling().Handle(repoBuilder.EmptyRepo().Git, GetTestConfigurationStub(), new[] { "log", "too", "many", "args" });

			Assert.False(logWasMatched);
			Assert.False(commitWasMatched);
		}

		[Fact]
		public void When_calling_with_specific_argumenthole_Then_match()
		{
			new CommandlineHandling().Handle(repoBuilder.EmptyRepo().Git, GetTestConfigurationStub(), new[] { "commit", "some message"});

			Assert.False(logWasMatched);
			Assert.True(commitWasMatched);
		}

		[Fact]
		public void When_not_calling_with_specific_argumenthole_Then_not_match()
		{
			new CommandlineHandling().Handle(repoBuilder.EmptyRepo().Git, GetTestConfigurationStub(), new[] { "commit" });

			Assert.False(logWasMatched);
			Assert.False(commitWasMatched);
		}
	}
}
