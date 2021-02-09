﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CO_Driver
{
    public partial class previous_match : UserControl
    {
        public file_trace_managment.MatchData last_match_data = new file_trace_managment.MatchData { };
        public file_trace_managment.BuildRecord last_build_record = new file_trace_managment.BuildRecord { };
        private string blue_team = "";
        private string red_team = "";
        private List<string> solo_queue_names = new List<string> { "A worthy collection of players.", 
                                                                   "The elite of solo queue.", 
                                                                   "Crossout's finest.",
                                                                   "Crossout's best and brightest.",
                                                                   "Worthy opponents"};
        public previous_match()
        {
            InitializeComponent();
        }
        public void populate_previous_match()
        {
            reset_screen_elements();
            assign_teams();
            TimeSpan duration = last_match_data.match_end - last_match_data.match_start;

            if (last_match_data.game_result == "Win")
                lb_game_result.Text = "Victory";
            else if (last_match_data.game_result == "Loss")
                lb_game_result.Text = "Defeat";
            else 
                lb_game_result.Text = last_match_data.game_result;

            lb_match_type.Text = last_match_data.match_type_desc;
            lb_map_name.Text = last_match_data.map_name;
            lb_build_name.Text = last_build_record.full_description;
            lb_duration.Text = string.Format(@"{0}M{1}s", duration.Minutes, duration.Seconds);
            lb_kills.Text = last_match_data.local_player.stats.kills.ToString();
            lb_assists.Text = last_match_data.local_player.stats.assists.ToString();
            lb_drone_kills.Text = last_match_data.local_player.stats.drone_kills.ToString();
            lb_damage_dealt.Text = Math.Round(last_match_data.local_player.stats.damage,1).ToString();
            lb_damage_rec.Text = Math.Round(last_match_data.local_player.stats.damage_taken,1).ToString();
            lb_score.Text = last_match_data.local_player.stats.score.ToString();
            lb_medals.Text = last_match_data.local_player.stripes.Count().ToString();
            if (last_match_data.match_rewards.ContainsKey("expFactionTotal")) {
                lb_xp.Text = last_match_data.match_rewards["expFactionTotal"].ToString();
                lb_resources.Text = string.Join(",", last_match_data.match_rewards.Where(x => x.Key.ToLower().Contains("exp") == false).Select(x => x.Key + ":" + x.Value.ToString()));
            }
            else
            {
                lb_xp.Text = "";
                lb_resources.Text = "";
            }
            
            lb_blue_team.Text = blue_team;
            lb_red_team.Text = red_team;

            populate_medals_table();
            populate_teams_tables();
            populate_damage_dealt();
            populate_damage_rec();
        }

        private void reset_screen_elements()
        {
            lb_game_result.Text = "";
            lb_match_type.Text = "";
            lb_map_name.Text = "";
            lb_build_name.Text = "";
            lb_duration.Text = "";
            lb_kills.Text = "";
            lb_assists.Text = "";
            lb_drone_kills.Text = "";
            lb_damage_dealt.Text = "";
            lb_damage_rec.Text = "";
            lb_score.Text = "";
            lb_medals.Text = "";
            lb_xp.Text = "";
            lb_resources.Text = "";
            lb_blue_team.Text = "";
            lb_red_team.Text = "";
            dg_damage_rec.Rows.Clear();
            dg_damage_dealt.Rows.Clear();
            dg_medals.Rows.Clear();
            dg_blue_team.Rows.Clear();
            dg_red_team.Rows.Clear();
            
        }

        private void populate_damage_rec()
        {
            dg_damage_rec.AllowUserToAddRows = false;
            dg_damage_rec.ClearSelection();
            dg_damage_rec.Rows.Clear();
            dg_damage_rec.AllowUserToAddRows = true;
            string previous_attacker = "";
            bool first = true;

            foreach (file_trace_managment.DamageRecord damage_record in last_match_data.damage_record.OrderBy(x => x.victim).ThenByDescending(x => x.damage).ToList())
            {
                if (damage_record.victim != last_match_data.local_player.nickname)
                    continue;

                DataGridViewRow row;

                if (damage_record.attacker != previous_attacker)
                {
                    if (!first)
                    {
                        row = (DataGridViewRow)this.dg_damage_rec.Rows[0].Clone();
                        row.Cells[0].Value = null;
                        row.Cells[1].Value = null;
                        dg_damage_rec.Rows.Add(row);
                    }

                    row = (DataGridViewRow)this.dg_damage_rec.Rows[0].Clone();
                    row.Cells[0].Style.Alignment = DataGridViewContentAlignment.BottomRight;
                    row.Cells[0].Value = damage_record.attacker;
                    row.Cells[1].Value = Math.Round(last_match_data.damage_record.Where(x => x.victim == damage_record.victim && x.attacker == damage_record.attacker).Sum(x => x.damage), 1);
                    dg_damage_rec.Rows.Add(row);
                }

                row = (DataGridViewRow)this.dg_damage_rec.Rows[0].Clone();
                row.Cells[0].Style.Alignment = DataGridViewContentAlignment.BottomRight;
                row.Cells[0].Value = damage_record.weapon;
                row.Cells[1].Value = Math.Round(damage_record.damage, 1);
                dg_damage_rec.Rows.Add(row);

                previous_attacker = damage_record.attacker;
                if (first)
                    first = false;
            }

            dg_damage_rec.AllowUserToAddRows = false;
            dg_damage_rec.ClearSelection();
        }

        private void populate_damage_dealt()
        {
            dg_damage_dealt.AllowUserToAddRows = false;
            dg_damage_dealt.ClearSelection();
            dg_damage_dealt.Rows.Clear();
            dg_damage_dealt.AllowUserToAddRows = true;
            string previous_victim = "";
            bool first = true;

            foreach (file_trace_managment.DamageRecord damage_record in last_match_data.damage_record.OrderBy(x=>x.victim).ThenByDescending(x=>x.damage).ToList())
            {
                if (damage_record.attacker != last_match_data.local_player.nickname)
                    continue;

                if (damage_record.victim == last_match_data.local_player.nickname)
                    continue;

                DataGridViewRow row;

                if (damage_record.victim != previous_victim)
                {
                    if (!first)
                    {
                        row = (DataGridViewRow)this.dg_damage_dealt.Rows[0].Clone();
                        row.Cells[0].Value = null;
                        row.Cells[1].Value = null;
                        dg_damage_dealt.Rows.Add(row);
                    }

                    row = (DataGridViewRow)this.dg_damage_dealt.Rows[0].Clone();
                    row.Cells[0].Style.Alignment = DataGridViewContentAlignment.BottomRight;
                    row.Cells[0].Value = damage_record.victim;
                    row.Cells[1].Value = Math.Round(last_match_data.damage_record.Where(x=>x.attacker == last_match_data.local_player.nickname && x.victim == damage_record.victim).Sum(x=>x.damage), 1);
                    dg_damage_dealt.Rows.Add(row);
                }

                row = (DataGridViewRow)this.dg_damage_dealt.Rows[0].Clone();
                row.Cells[0].Style.Alignment = DataGridViewContentAlignment.BottomRight;
                row.Cells[0].Value = damage_record.weapon;
                row.Cells[1].Value = Math.Round(damage_record.damage, 1);
                dg_damage_dealt.Rows.Add(row);

                previous_victim = damage_record.victim;
                if (first)
                    first = false;
            }

            dg_damage_dealt.AllowUserToAddRows = false;
            dg_damage_dealt.ClearSelection();
        }

        private void populate_medals_table()
        {
            dg_medals.AllowUserToAddRows = false;
            dg_medals.ClearSelection();
            dg_medals.Rows.Clear();
            dg_medals.AllowUserToAddRows = true;

            Dictionary<string, int> stripes = new Dictionary<string, int> { };
            foreach (string stripe in last_match_data.local_player.stripes.OrderBy(x => x))
            {
                if (stripes.ContainsKey(stripe))
                    stripes[stripe] += 1;
                else
                    stripes.Add(stripe, 1);
            }

            foreach (KeyValuePair<string, int> stripe in stripes)
            {
                DataGridViewRow row = (DataGridViewRow)this.dg_medals.Rows[0].Clone();
                row.Cells[0].Value = stripe.Key;
                row.Cells[1].Value = stripe.Value;
                dg_medals.Rows.Add(row);
            }

            dg_medals.AllowUserToAddRows = false;
            dg_medals.ClearSelection();
        }

        private void populate_teams_tables()
        {
            dg_blue_team.AllowUserToAddRows = false;
            dg_red_team.AllowUserToAddRows = false;

            dg_blue_team.Rows.Clear();
            dg_blue_team.AllowUserToAddRows = true;

            dg_red_team.Rows.Clear();
            dg_red_team.AllowUserToAddRows = true;


            foreach (KeyValuePair<string, file_trace_managment.Player> player in last_match_data.player_records.ToList())
            {
                if (player.Value.team != last_match_data.local_player.team)
                {
                    DataGridViewRow row = (DataGridViewRow)this.dg_red_team.Rows[0].Clone();
                    row.Cells[0].Value = player.Key;
                    row.Cells[1].Value = player.Value.stats.kills;
                    row.Cells[2].Value = player.Value.stats.assists;
                    row.Cells[3].Value = player.Value.stats.drone_kills;
                    row.Cells[4].Value = Math.Round(player.Value.stats.damage, 1);
                    row.Cells[5].Value = Math.Round(player.Value.stats.damage_taken, 1);
                    row.Cells[6].Value = player.Value.stats.score;
                    dg_red_team.Rows.Add(row);
                }
                else
                {
                    DataGridViewRow row = (DataGridViewRow)this.dg_blue_team.Rows[0].Clone();
                    row.Cells[0].Value = player.Key;
                    row.Cells[1].Value = player.Value.stats.kills;
                    row.Cells[2].Value = player.Value.stats.assists;
                    row.Cells[3].Value = player.Value.stats.drone_kills;
                    row.Cells[4].Value = Math.Round(player.Value.stats.damage, 1);
                    row.Cells[5].Value = Math.Round(player.Value.stats.damage_taken, 1);
                    row.Cells[6].Value = player.Value.stats.score;
                    dg_blue_team.Rows.Add(row);
                }
            }

            dg_blue_team.AllowUserToAddRows = false;
            dg_red_team.AllowUserToAddRows = false;
            

            dg_red_team.Sort(dg_red_team.Columns[6], ListSortDirection.Descending);
            dg_blue_team.Sort(dg_blue_team.Columns[6], ListSortDirection.Descending);

            dg_blue_team.ClearSelection();
            dg_red_team.ClearSelection();
        }

        private void assign_teams()
        {
            blue_team = "";
            Random random_number = new Random();

            if (last_match_data.match_type == global_data.EASY_RAID_MATCH ||
                last_match_data.match_type == global_data.MED_RAID_MATCH ||
                last_match_data.match_type == global_data.HARD_RAID_MATCH ||
                last_match_data.match_type == global_data.ADVENTURE_MATCH ||
                last_match_data.match_type == global_data.PRESENT_HEIST_MATCH ||
                last_match_data.match_type == global_data.PATROL_MATCH)
                red_team = "A bunch of robots.";
            else
            if (last_match_data.match_type == global_data.BEDLAM_MATCH)
                red_team = "The elite of Bedlam.";
            else
                red_team = solo_queue_names[random_number.Next(solo_queue_names.Count())];

            Dictionary<int, List<string>> blue_teams = new Dictionary<int, List<string>> { };
            Dictionary<int, List<string>> red_teams = new Dictionary<int, List<string>> { };

            blue_teams.Add(last_match_data.local_player.party_id, new List<string> { last_match_data.local_player.nickname });

            foreach (KeyValuePair<string, file_trace_managment.Player> player in last_match_data.player_records.ToList())
            {
                if (player.Value.party_id == 0 || player.Value.nickname == last_match_data.local_player.nickname)
                    continue;

                if (player.Value.team != last_match_data.local_player.team)
                {
                    if (!red_teams.ContainsKey(player.Value.party_id))
                        red_teams.Add(player.Value.party_id, new List<string> { player.Value.nickname });
                    else
                        red_teams[player.Value.party_id].Add(player.Value.nickname);
                }
                else
                {
                    if (!blue_teams.ContainsKey(player.Value.party_id))
                        blue_teams.Add(player.Value.party_id, new List<string> { player.Value.nickname });
                    else
                        blue_teams[player.Value.party_id].Add(player.Value.nickname);
                }
            }

            foreach (KeyValuePair<int, List<string>> team in blue_teams)
                blue_team += string.Format("({0})", string.Join(",", team.Value));

            if (red_teams.Count > 0)
                red_team = "";

            foreach (KeyValuePair<int, List<string>> team in red_teams)
                red_team += string.Format("({0})", string.Join(",", team.Value));
        }

        private void gp_damage_recieved_Paint(object sender, PaintEventArgs e)
        {
            GroupBox box = sender as GroupBox;
            draw_group_box(box, e.Graphics, Color.Lime, Color.Lime);
        }

        private void gp_medals_Paint(object sender, PaintEventArgs e)
        {
            GroupBox box = sender as GroupBox;
            draw_group_box(box, e.Graphics, Color.Lime, Color.Lime);
        }

        private void gb_red_team_Paint(object sender, PaintEventArgs e)
        {
            GroupBox box = sender as GroupBox;
            draw_group_box(box, e.Graphics, Color.Lime, Color.Lime);
        }

        private void gb_blue_team_Paint(object sender, PaintEventArgs e)
        {
            GroupBox box = sender as GroupBox;
            draw_group_box(box, e.Graphics, Color.Lime, Color.Lime);
        }

        private void gb_damage_dealt_Paint(object sender, PaintEventArgs e)
        {
            GroupBox box = sender as GroupBox;
            draw_group_box(box, e.Graphics, Color.Lime, Color.Lime);
        }

        private void draw_group_box(GroupBox box, Graphics g, Color textColor, Color borderColor)
        {
            if (box != null)
            {
                Brush textBrush = new SolidBrush(textColor);
                Brush borderBrush = new SolidBrush(borderColor);
                Pen borderPen = new Pen(borderBrush);
                SizeF strSize = g.MeasureString(box.Text, box.Font);
                Rectangle rect = new Rectangle(box.ClientRectangle.X,
                                               box.ClientRectangle.Y + (int)(strSize.Height / 2),
                                               box.ClientRectangle.Width - 1,
                                               box.ClientRectangle.Height - (int)(strSize.Height / 2) - 1);


                g.Clear(this.BackColor);
                g.DrawString(box.Text, box.Font, textBrush, box.Padding.Left, 0);

                g.DrawLine(borderPen, rect.Location, new Point(rect.X, rect.Y + rect.Height));
                g.DrawLine(borderPen, new Point(rect.X + rect.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                g.DrawLine(borderPen, new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                g.DrawLine(borderPen, new Point(rect.X, rect.Y), new Point(rect.X + box.Padding.Left, rect.Y));
                g.DrawLine(borderPen, new Point(rect.X + box.Padding.Left + (int)(strSize.Width), rect.Y), new Point(rect.X + rect.Width, rect.Y));
            }
        }
    }
}