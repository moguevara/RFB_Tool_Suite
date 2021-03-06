﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using System.Windows.Forms.DataVisualization.Charting;

namespace CO_Driver
{
    public partial class meta_detail : UserControl
    {
        public List<file_trace_managment.MatchRecord> match_history = new List<file_trace_managment.MatchRecord> { };
        public Dictionary<string, file_trace_managment.BuildRecord> build_records = new Dictionary<string, file_trace_managment.BuildRecord> { };
        public log_file_managment.session_variables session = new log_file_managment.session_variables { };
        public Dictionary<string, Dictionary<string, translate.Translation>> translations;
        public Dictionary<string, Dictionary<string, string>> ui_translations = new Dictionary<string, Dictionary<string, string>> { };
        public bool force_refresh = false;
        private Dictionary<string, int> weapon_usage = new Dictionary<string, int> { };
        private Dictionary<string, int> movement_usage = new Dictionary<string, int> { };
        private Dictionary<string, int> cabin_usage = new Dictionary<string, int> { };
        private Dictionary<string, int> module_usage = new Dictionary<string, int> { };
        private filter.FilterSelections filter_selections = filter.new_filter_selection();
        private string new_selection = "";
        private string previous_selection = "";
        private int total_games = 0;
        private int total_wins = 0;
        private double global_enemy_win_percent = 0.0;


        private List<master_meta_grouping> master_groupings = new List<master_meta_grouping> { };
        private List<meta_grouping> match_stats = new List<meta_grouping> { };

        private class meta_grouping
        {
            public string weapon { get; set; }
            public string cabin { get; set; }
            public string movement { get; set; }
            public string map { get; set; }
            public file_trace_managment.Stats stats { get; set; }
        }

        private class master_meta_grouping
        {
            public int games { get; set; }
            public meta_grouping group { get; set; }
        }

        public meta_detail()
        {
            InitializeComponent();
        }

        public void populate_meta_detail_screen()
        {
            if (!force_refresh)
            {
                new_selection = filter.filter_string(filter_selections);

                if (new_selection == previous_selection)
                    return;
            }

            force_refresh = false;

            total_games = 0;
            total_wins = 0;
            master_groupings = new List<master_meta_grouping> { };
            List<meta_grouping> groupings = new List<meta_grouping> { };

            previous_selection = new_selection;

            filter.reset_filters(filter_selections);
            initialize_user_profile();

            foreach (file_trace_managment.MatchRecord match in match_history.ToList())
            {

                if (!filter.check_filters(filter_selections, match, build_records, session, translations))
                    continue;

                /* begin calc */

                total_games += 1;
                match_stats = new List<meta_grouping> { };
                

                if (match.match_data.local_player.team != match.match_data.winning_team && match.match_data.winning_team != -1)
                    total_wins += 1; /* enemy wins */

                foreach (KeyValuePair<string, file_trace_managment.Player> player in match.match_data.player_records)
                {
                    if (player.Value.team == match.match_data.local_player.team)
                        continue;

                    if (!build_records.ContainsKey(player.Value.build_hash))
                        continue;

                    if (!chk_bot_filter.Checked && player.Value.bot == 1)
                        continue;

                    if (!chk_bot_filter.Checked && player.Value.power_score >= 22000)
                        continue;

                    groupings = new List<meta_grouping> { };
                    file_trace_managment.BuildRecord build = build_records[player.Value.build_hash];

                    if (chk_weapon_filter.Checked)
                    {
                        foreach (part_loader.Weapon weapon in build.weapons)
                        {
                            meta_grouping new_group = new_grouping();
                            new_group.weapon = translate.translate_string(weapon.name, session, translations);
                            new_group.stats = player.Value.stats;
                            groupings.Add(new_group);
                        }
                    }

                    if (chk_movement_filter.Checked)
                    {
                        if (groupings == null || !groupings.Any())
                        {
                            foreach (part_loader.Movement movement in build.movement)
                            {
                                meta_grouping new_group = new_grouping();
                                new_group.movement = translate.translate_string(movement.name, session, translations);
                                new_group.stats = player.Value.stats;
                                groupings.Add(new_group);
                            }
                        }
                        else
                        {
                            foreach (meta_grouping sub_group in groupings.ToList())
                            {
                                for(int i = 0; i < build.movement.Count(); i++)
                                {
                                    if (i == 0)
                                    {
                                        sub_group.movement = translate.translate_string(build.movement[i].name, session, translations);
                                    }
                                    else
                                    {
                                        meta_grouping new_group = new_grouping();
                                        new_group.weapon = sub_group.weapon;
                                        new_group.movement = translate.translate_string(build.movement[i].name, session, translations);
                                        new_group.stats = player.Value.stats;
                                        groupings.Add(new_group);
                                    }
                                }
                            }
                        }
                    }

                    if (chk_cabin_filter.Checked)
                    {
                        if (groupings == null || !groupings.Any())
                        {
                            meta_grouping new_group = new_grouping();
                            new_group.cabin = translate.translate_string(build.cabin.name, session, translations);
                            new_group.stats = player.Value.stats;
                            groupings.Add(new_group);
                        }
                        else
                        {
                            foreach (meta_grouping sub_group in groupings)
                                sub_group.cabin = translate.translate_string(build.cabin.name, session, translations);
                        }
                    }
                        

                    if (chk_map_filter.Checked)
                    {
                        if (groupings == null || !groupings.Any())
                        {
                            meta_grouping new_group = new_grouping();
                            new_group.map = translate.translate_string(match.match_data.map_name, session, translations);
                            new_group.stats = player.Value.stats;
                            groupings.Add(new_group);
                        }
                        else
                        {
                            foreach (meta_grouping sub_group in groupings)
                                sub_group.map = translate.translate_string(match.match_data.map_name, session, translations);
                        }
                    }

                    foreach (meta_grouping sub_group in groupings)
                    {
                        bool found = false;
                        for (int i = 0; i < match_stats.Count(); i++)
                        {
                            if (match_stats[i].cabin == sub_group.cabin &&
                                match_stats[i].movement == sub_group.movement &&
                                match_stats[i].weapon == sub_group.weapon &&
                                match_stats[i].map == sub_group.map)
                            {
                                found = true;
                                match_stats[i].stats = file_trace_managment.sum_stats(match_stats[i].stats, player.Value.stats);
                            }
                        }
                        if (!found)
                            match_stats.Add(sub_group);
                    }
                }
                foreach (meta_grouping sub_group in match_stats)
                {
                    bool found = false;
                    for (int i = 0; i < master_groupings.Count(); i++)
                    {
                        if (master_groupings[i].group.cabin == sub_group.cabin &&
                            master_groupings[i].group.movement == sub_group.movement &&
                            master_groupings[i].group.weapon == sub_group.weapon &&
                            master_groupings[i].group.map == sub_group.map)
                        {
                            found = true;
                            master_groupings[i].games += 1;
                            master_groupings[i].group.stats = file_trace_managment.sum_stats(master_groupings[i].group.stats, sub_group.stats);
                        }
                    }
                    if (!found)
                        master_groupings.Add(new master_meta_grouping { games = 1, group = sub_group });
                }

            }

            global_enemy_win_percent = (double)total_games > 0 ? (double)total_wins / (double)total_games : 0.0;

            lb_global_percentage.Text = string.Format("{0}%", Math.Round(global_enemy_win_percent * 100, 1));
            lb_total_game.Text = total_games.ToString();

            populate_meta_detail_screen_elements();
            filter.populate_filters(filter_selections, cb_game_modes, cb_grouped, cb_power_score, cb_versions, cb_weapons, cb_movement, cb_cabins, cb_modules);
        }

        private meta_grouping new_grouping()
        {
            return new meta_grouping { map = "", cabin = "", movement = "", weapon = "" };
        }
        private void populate_meta_detail_screen_elements()
        {
            dg_meta_detail_view.Rows.Clear();
            dg_meta_detail_view.Columns[1].DefaultCellStyle.Format = "MM/dd HH:mm:ss";
            dg_meta_detail_view.AllowUserToAddRows = true;

            dg_meta_detail_view.Columns[4].DefaultCellStyle.Format = "N0";
            dg_meta_detail_view.Columns[5].DefaultCellStyle.Format = "P1";
            dg_meta_detail_view.Columns[6].DefaultCellStyle.Format = "N2";
            dg_meta_detail_view.Columns[7].DefaultCellStyle.Format = "N2";
            dg_meta_detail_view.Columns[8].DefaultCellStyle.Format = "N2";
            dg_meta_detail_view.Columns[9].DefaultCellStyle.Format = "N2";
            dg_meta_detail_view.Columns[10].DefaultCellStyle.Format = "N1";
            dg_meta_detail_view.Columns[11].DefaultCellStyle.Format = "N1";
            dg_meta_detail_view.Columns[12].DefaultCellStyle.Format = "N0";
            dg_meta_detail_view.Columns[13].DefaultCellStyle.Format = "P2";
            dg_meta_detail_view.Columns[14].DefaultCellStyle.Format = "P2";

            foreach (master_meta_grouping group in master_groupings)
            {
                DataGridViewRow row = (DataGridViewRow)dg_meta_detail_view.Rows[0].Clone();
                row.Cells[0].Value = group.group.weapon;
                row.Cells[1].Value = group.group.cabin;
                row.Cells[2].Value = group.group.movement;
                row.Cells[3].Value = group.group.map;
                row.Cells[4].Value = group.games;
                row.Cells[5].Value = (double)group.games / (double)total_games;
                row.Cells[6].Value = (double)group.group.stats.games / (double)group.games;
                row.Cells[7].Value = (double)group.group.stats.kills / (double)group.group.stats.rounds;
                row.Cells[8].Value = (double)group.group.stats.assists / (double)group.group.stats.rounds;
                row.Cells[9].Value = (double)group.group.stats.deaths / (double)group.group.stats.rounds;
                row.Cells[10].Value = (double)group.group.stats.damage / (double)group.group.stats.rounds;
                row.Cells[11].Value = (double)group.group.stats.damage_taken / (double)group.group.stats.rounds;
                row.Cells[12].Value = (double)group.group.stats.score / (double)group.group.stats.rounds;
                row.Cells[13].Value = (double)group.group.stats.wins / (double)group.group.stats.games;
                row.Cells[14].Value = (((double)group.group.stats.wins / (double)group.group.stats.games) - global_enemy_win_percent);
                dg_meta_detail_view.Rows.Add(row);
            }

            dg_meta_detail_view.AllowUserToAddRows = false;
            dg_meta_detail_view.Sort(dg_meta_detail_view.Columns[5], ListSortDirection.Descending);
        }

        private void initialize_user_profile()
        {
        }
        private void btn_save_user_settings_Click_1(object sender, EventArgs e)
        {
            filter.reset_filter_selections(filter_selections);

            chk_weapon_filter.Checked = true;
            chk_bot_filter.Checked = false;
            chk_cabin_filter.Checked = false;
            chk_map_filter.Checked = false;
            chk_movement_filter.Checked = false;

            dt_start_date.Value = DateTime.Now;
            dt_end_date.Value = DateTime.Now;

            populate_meta_detail_screen();
        }

        private void cb_versions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_versions.SelectedIndex >= 0)
                filter_selections.client_versions_filter = this.cb_versions.Text;

            populate_meta_detail_screen();
        }

        private void cb_power_score_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_power_score.SelectedIndex >= 0)
                filter_selections.power_score_filter = this.cb_power_score.Text;

            populate_meta_detail_screen();
        }

        private void cb_grouped_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_grouped.SelectedIndex >= 0)
                filter_selections.group_filter = this.cb_grouped.Text;

            populate_meta_detail_screen();
        }

        private void cb_game_modes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_game_modes.SelectedIndex >= 0)
                filter_selections.game_mode_filter = this.cb_game_modes.Text;

            populate_meta_detail_screen();
        }

        private void dt_start_date_ValueChanged(object sender, EventArgs e)
        {
            filter_selections.start_date = dt_start_date.Value;
            populate_meta_detail_screen();
        }

        private void dt_end_date_ValueChanged(object sender, EventArgs e)
        {
            filter_selections.end_date = dt_end_date.Value;
            populate_meta_detail_screen();
        }

        private void cb_cabins_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_cabins.SelectedIndex >= 0)
                filter_selections.cabin_filter = this.cb_cabins.Text;

            populate_meta_detail_screen();
        }

        private void cb_weapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_weapons.SelectedIndex >= 0)
                filter_selections.weapons_filter = this.cb_weapons.Text;

            populate_meta_detail_screen();
        }

        private void cb_modules_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_modules.SelectedIndex >= 0)
                filter_selections.module_filter = this.cb_modules.Text;

            populate_meta_detail_screen();
        }

        private void cb_movement_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_movement.SelectedIndex >= 0)
                filter_selections.movement_filter = this.cb_movement.Text;

            populate_meta_detail_screen();
        }

        private void chk_weapon_filter_CheckedChanged(object sender, EventArgs e)
        {
            force_refresh = true;
            populate_meta_detail_screen();
        }

        private void chk_cabin_filter_CheckedChanged(object sender, EventArgs e)
        {
            force_refresh = true;
            populate_meta_detail_screen();
        }

        private void chk_movement_filter_CheckedChanged(object sender, EventArgs e)
        {
            force_refresh = true;
            populate_meta_detail_screen();
        }

        private void chk_map_filter_CheckedChanged(object sender, EventArgs e)
        {
            force_refresh = true;
            populate_meta_detail_screen();
        }

        private void chk_bot_filter_CheckedChanged(object sender, EventArgs e)
        {
            force_refresh = true;
            populate_meta_detail_screen();
        }

        private void lb_user_name_Click(object sender, EventArgs e)
        {

        }
    }
}
