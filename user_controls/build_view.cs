﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using CO_Driver.Properties;
using System.Globalization;

namespace CO_Driver
{
    public partial class build_view : UserControl
    {
        private class BuildStats
        {
            public string build_hash;
            public int power_score;
            public file_trace_managment.Stats stats;
        }

        public file_trace_managment.MatchHistoryResponse match_history = new file_trace_managment.MatchHistoryResponse { };
        public Dictionary<string, file_trace_managment.BuildRecord> build_records = new Dictionary<string, file_trace_managment.BuildRecord> { };
        public Dictionary<string, Dictionary<string, translate.Translation>> translations;
        public Dictionary<string, Dictionary<string, string>> ui_translations = new Dictionary<string, Dictionary<string, string>> { };
        private Dictionary<string, BuildStats> build_stats = new Dictionary<string, BuildStats> { };
        public log_file_managment.session_variables session = new log_file_managment.session_variables { };
        private string game_mode_filter = global_data.GAME_MODE_FILTER_DEFAULT;
        private string group_filter = global_data.GROUP_FILTER_DEFAULT;
        private string map_filter = global_data.MAP_FILTER_DEFAULT;
        private string power_score_filter = global_data.POWER_SCORE_FILTER_DEFAULT;
        private string client_versions_filter = global_data.CLIENT_VERSION_FILTER_DEFAULT;
        private string new_selection = "";
        private string previous_selection = "";

        private List<string> game_modes = new List<string> { };
        private List<string> grouped = new List<string> { };
        private List<string> maps = new List<string> { };
        private List<string> power_scores = new List<string> { };
        private List<string> client_versions = new List<string> { };

        public build_view()
        {
            InitializeComponent();

        }

        private void populate_build_records()
        {
            //translate.translate_string(map.Key,session, translations);
            //ui_translate.translate(ctrl.Text, session, ui_translations);

            build_stats = new Dictionary<string, BuildStats> { };

            reset_filters();

            foreach (file_trace_managment.MatchRecord match in match_history.match_history.ToList())
            {
                if (game_mode_filter != global_data.GAME_MODE_FILTER_DEFAULT && game_mode_filter != match.match_data.match_type_desc)
                    continue;

                if (group_filter == "Solo" &&  match.match_data.local_player.party_id > 0)
                    continue;

                if (group_filter == "Grouped" &&  match.match_data.local_player.party_id == 0)
                    continue;

                if (map_filter != global_data.MAP_FILTER_DEFAULT && map_filter != translate.translate_string(match.match_data.map_name, session, translations))
                    continue;

                if (client_versions_filter != global_data.CLIENT_VERSION_FILTER_DEFAULT && client_versions_filter != match.match_data.client_version)
                    continue;

                if (power_score_filter != global_data.POWER_SCORE_FILTER_DEFAULT)
                {
                    if (power_score_filter == "0-2499" && ( match.match_data.local_player.power_score < 0 ||  match.match_data.local_player.power_score > 2499))
                        continue;

                    if (power_score_filter == "2500-3499" && ( match.match_data.local_player.power_score < 2500 ||  match.match_data.local_player.power_score > 3499))
                        continue;

                    if (power_score_filter == "3500-4499" && ( match.match_data.local_player.power_score < 3500 ||  match.match_data.local_player.power_score > 4499))
                        continue;

                    if (power_score_filter == "4500-5499" && ( match.match_data.local_player.power_score < 4500 ||  match.match_data.local_player.power_score > 5499))
                        continue;

                    if (power_score_filter == "5500-6499" && ( match.match_data.local_player.power_score < 5500 ||  match.match_data.local_player.power_score > 6499))
                        continue;

                    if (power_score_filter == "6500-7499" && ( match.match_data.local_player.power_score < 6500 ||  match.match_data.local_player.power_score > 7499))
                        continue;

                    if (power_score_filter == "7500-8499" && ( match.match_data.local_player.power_score < 7500 ||  match.match_data.local_player.power_score > 8499))
                        continue;

                    if (power_score_filter == "8500-9499" && ( match.match_data.local_player.power_score < 8500 ||  match.match_data.local_player.power_score > 9499))
                        continue;

                    if (power_score_filter == "9500-12999" && ( match.match_data.local_player.power_score < 9500 ||  match.match_data.local_player.power_score > 12999))
                        continue;

                    if (power_score_filter == "13000+" && ( match.match_data.local_player.power_score < 13000 ||  match.match_data.local_player.power_score > 22000))
                        continue;

                    if (power_score_filter == "Leviathian" &&  match.match_data.local_player.power_score < 22000)
                        continue;
                }

                if (!game_modes.Contains(match.match_data.match_type_desc))
                    game_modes.Add((match.match_data.match_type_desc));

                if ( match.match_data.local_player.party_id == 0 && !grouped.Contains("Solo"))
                    grouped.Add("Solo");

                if ( match.match_data.local_player.party_id > 0 && !grouped.Contains("Grouped"))
                    grouped.Add("Grouped");

                if (!maps.Contains(translate.translate_string(match.match_data.map_name, session, translations)))
                    maps.Add(translate.translate_string(match.match_data.map_name, session, translations));

                if ( match.match_data.local_player.power_score >= 0 &&  match.match_data.local_player.power_score <= 2499 && !power_scores.Contains("0-2499"))
                    power_scores.Add("0-2499");

                if ( match.match_data.local_player.power_score >= 2500 &&  match.match_data.local_player.power_score <= 2499 && !power_scores.Contains("2500-3499"))
                    power_scores.Add("2500-3499");

                if ( match.match_data.local_player.power_score >= 3500 &&  match.match_data.local_player.power_score <= 4499 && !power_scores.Contains("3500-4499"))
                    power_scores.Add("3500-4499");

                if ( match.match_data.local_player.power_score >= 4500 &&  match.match_data.local_player.power_score <= 5499 && !power_scores.Contains("4500-5499"))
                    power_scores.Add("4500-5499");

                if ( match.match_data.local_player.power_score >= 5500 &&  match.match_data.local_player.power_score <= 6499 && !power_scores.Contains("5500-6499"))
                    power_scores.Add("5500-6499");

                if ( match.match_data.local_player.power_score >= 6500 &&  match.match_data.local_player.power_score <= 7499 && !power_scores.Contains("6500-7499"))
                    power_scores.Add("6500-7499");

                if ( match.match_data.local_player.power_score >= 7500 &&  match.match_data.local_player.power_score <= 8499 && !power_scores.Contains("7500-8499"))
                    power_scores.Add("7500-8499");

                if ( match.match_data.local_player.power_score >= 8500 &&  match.match_data.local_player.power_score <= 9499 && !power_scores.Contains("8500-9499"))
                    power_scores.Add("8500-9499");

                if ( match.match_data.local_player.power_score >= 9500 &&  match.match_data.local_player.power_score <= 12999 && !power_scores.Contains("9500-12999"))
                    power_scores.Add("9500-12999");

                if ( match.match_data.local_player.power_score >= 13000 &&  match.match_data.local_player.power_score <= 22000 && !power_scores.Contains("13000+"))
                    power_scores.Add("13000+");

                if ( match.match_data.local_player.power_score >= 22000 && !power_scores.Contains("Leviathian"))
                    power_scores.Add("Leviathian");

                if (!client_versions.Contains(match.match_data.client_version))
                    client_versions.Add((match.match_data.client_version));

                if (!build_stats.ContainsKey( match.match_data.local_player.build_hash))
                {
                    build_stats.Add( match.match_data.local_player.build_hash, new BuildStats { build_hash =  match.match_data.local_player.build_hash, power_score =  match.match_data.local_player.power_score, stats =  match.match_data.local_player.stats });
                }
                else
                {
                    build_stats[ match.match_data.local_player.build_hash].stats = file_trace_managment.sum_stats(build_stats[ match.match_data.local_player.build_hash].stats,  match.match_data.local_player.stats);
                }
            }
        }

        private void reset_filters()
        {
            game_modes = new List<string> { };
            grouped = new List<string> { };
            maps = new List<string> { };
            power_scores = new List<string> { };
            client_versions = new List<string> { };

            game_modes.Add(global_data.GAME_MODE_FILTER_DEFAULT);
            grouped.Add(global_data.GROUP_FILTER_DEFAULT);
            maps.Add(global_data.MAP_FILTER_DEFAULT);
            power_scores.Add(global_data.POWER_SCORE_FILTER_DEFAULT);
            client_versions.Add(global_data.CLIENT_VERSION_FILTER_DEFAULT);
        }

        private void populate_filters()
        {
            cb_build_game_modes.Items.Clear();
            cb_grouped.Items.Clear();
            cb_map.Items.Clear();
            cb_power_score.Items.Clear();
            cb_client_version.Items.Clear();

            game_modes = game_modes.OrderBy(x => x != global_data.GAME_MODE_FILTER_DEFAULT).ThenBy(x => x).ToList();
            maps = maps.OrderBy(x => x != global_data.MAP_FILTER_DEFAULT).ThenBy(x => x).ToList();
            power_scores = power_scores.OrderBy(x => x != global_data.POWER_SCORE_FILTER_DEFAULT).ThenBy(x => x).ToList();
            client_versions = client_versions.OrderBy(x => x != global_data.CLIENT_VERSION_FILTER_DEFAULT).ThenBy(x => x).ToList();

            if (power_scores.Contains("13000+"))
            {
                power_scores.Remove("13000+");
                power_scores.Add("13000+");
            }

            if (power_scores.Contains("Leviathian"))
            {
                power_scores.Remove("Leviathian");
                power_scores.Add("Leviathian");
            }

            foreach (string desc in game_modes)
                cb_build_game_modes.Items.Add(desc ?? "");

            foreach (string desc in grouped)
                cb_grouped.Items.Add(desc ?? "");

            foreach (string desc in maps)
                cb_map.Items.Add(desc ?? "");

            foreach (string desc in power_scores)
                cb_power_score.Items.Add(desc ?? "");

            foreach (string desc in client_versions)
                cb_client_version.Items.Add(desc ?? "");

            cb_build_game_modes.Text = game_mode_filter;
            cb_grouped.Text = group_filter;
            cb_map.Text = map_filter;
            cb_power_score.Text = power_score_filter;
            cb_client_version.Text = client_versions_filter;
        }

        public void populate_build_record_table()
        {
            new_selection = string.Format(@"{0},{1},{2},{3},{4}", game_mode_filter, group_filter, map_filter, power_score_filter, client_versions_filter);

            if (new_selection == previous_selection)
                return;

            previous_selection = new_selection;

            populate_build_records();
            this.dg_build_view_grid.Rows.Clear();
            this.dg_build_view_grid.AllowUserToAddRows = true;
            int rows_populated = 0;

            this.tb_build_description.Text = "";
            this.tb_short_desc.Text = "";
            this.tb_cabin.Text = "";
            this.tb_modules.Text = "";
            this.tb_weapons.Text = "";
            this.tb_movement.Text = "";
            this.tb_modules.Text = "";
            this.tb_modules.Text = "";
            this.tb_modules.Text = "";
            this.tb_build_parts.Text = "";

            foreach (KeyValuePair<string, BuildStats> build in build_stats)
            {
                DataGridViewRow row = (DataGridViewRow)this.dg_build_view_grid.Rows[0].Clone();
                row.Cells[0].Value = build.Key;
                row.Cells[1].Value = build.Value.power_score;
                row.Cells[2].Value = build.Value.stats.games;
                row.Cells[3].Value = build.Value.stats.kills;
                row.Cells[4].Value = Math.Round(build.Value.stats.deaths > 0 ? (double)build.Value.stats.kills / (double)build.Value.stats.deaths : Double.PositiveInfinity, 2);
                row.Cells[5].Value = Math.Round((double)build.Value.stats.damage / (double)build.Value.stats.rounds, 1);
                row.Cells[6].Value = Math.Round((double)build.Value.stats.damage_taken / (double)build.Value.stats.rounds, 1);
                row.Cells[7].Value = build.Value.stats.wins;
                row.Cells[8].Value = Math.Round(build.Value.stats.losses > 0 ? (double)build.Value.stats.wins / (double)build.Value.stats.games : 1.0, 2);


                this.dg_build_view_grid.Rows.Add(row);
                rows_populated++;
            }
            if (rows_populated > 0 && build_records.ContainsKey(this.dg_build_view_grid.Rows[0].Cells[0].Value.ToString()))
            {
                this.dg_build_view_grid.Sort(this.dg_build_view_grid.Columns[2], ListSortDirection.Descending);

                this.tb_build_description.Text = build_records[this.dg_build_view_grid.Rows[0].Cells[0].Value.ToString()].full_description;
                this.tb_short_desc.Text = build_records[this.dg_build_view_grid.Rows[0].Cells[0].Value.ToString()].short_description;
                this.tb_cabin.Text = translate.translate_string(build_records[this.dg_build_view_grid.Rows[0].Cells[0].Value.ToString()].cabin.name, session, translations);
                this.tb_modules.Text = translate.translate_string(build_records[this.dg_build_view_grid.Rows[0].Cells[0].Value.ToString()].engine.name, session, translations);
                this.tb_weapons.Text = string.Join(",", build_records[this.dg_build_view_grid.Rows[0].Cells[0].Value.ToString()].weapons.Select(x => translate.translate_string(x.name, session, translations)));
                this.tb_movement.Text = string.Join(",", build_records[this.dg_build_view_grid.Rows[0].Cells[0].Value.ToString()].movement.Select(x => translate.translate_string(x.name, session, translations)));
                this.tb_modules.Text = string.Join(",", build_records[this.dg_build_view_grid.Rows[0].Cells[0].Value.ToString()].modules.Select(x => translate.translate_string(x.name, session, translations)));
                this.tb_modules.Text += string.Join(",", translate.translate_string(build_records[this.dg_build_view_grid.Rows[0].Cells[0].Value.ToString()].engine.name, session, translations));
                this.tb_modules.Text += string.Join(",", build_records[this.dg_build_view_grid.Rows[0].Cells[0].Value.ToString()].explosives.Select(x => translate.translate_string(x.name, session, translations)));
                this.tb_build_parts.Text = string.Join(",", build_records[this.dg_build_view_grid.Rows[0].Cells[0].Value.ToString()].parts.Select(x => translate.translate_string(x, session, translations)));
            }

            this.dg_build_view_grid.AllowUserToAddRows = false;
            this.dg_build_view_grid.Sort(this.dg_build_view_grid.Columns[2], ListSortDirection.Descending);
            this.lb_build_header.Focus();
            this.dg_build_view_grid.ClearSelection();
            populate_filters();
        }

        private void dg_build_view_grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            this.tb_build_parts.Clear();

            if (this.dg_build_view_grid.Rows.Count > 0 && e.RowIndex >= 0)
            {
                this.tb_build_description.Text = build_records[this.dg_build_view_grid.Rows[e.RowIndex].Cells[0].Value.ToString()].full_description;
                this.tb_short_desc.Text = build_records[this.dg_build_view_grid.Rows[e.RowIndex].Cells[0].Value.ToString()].short_description;
                this.tb_cabin.Text = translate.translate_string(build_records[this.dg_build_view_grid.Rows[e.RowIndex].Cells[0].Value.ToString()].cabin.name, session, translations);
                this.tb_modules.Text = translate.translate_string(build_records[this.dg_build_view_grid.Rows[e.RowIndex].Cells[0].Value.ToString()].engine.name, session, translations);
                this.tb_weapons.Text = string.Join(",", build_records[this.dg_build_view_grid.Rows[e.RowIndex].Cells[0].Value.ToString()].weapons.Select(x => translate.translate_string(x.name, session, translations)));
                this.tb_movement.Text = string.Join(",", build_records[this.dg_build_view_grid.Rows[e.RowIndex].Cells[0].Value.ToString()].movement.Select(x => translate.translate_string(x.name, session, translations)));
                this.tb_modules.Text = string.Join(",", build_records[this.dg_build_view_grid.Rows[e.RowIndex].Cells[0].Value.ToString()].modules.Select(x => translate.translate_string(x.name, session, translations)));
                this.tb_modules.Text += string.Join(",", translate.translate_string(build_records[this.dg_build_view_grid.Rows[e.RowIndex].Cells[0].Value.ToString()].engine.name, session, translations));
                this.tb_modules.Text += string.Join(",", build_records[this.dg_build_view_grid.Rows[e.RowIndex].Cells[0].Value.ToString()].explosives.Select(x => translate.translate_string(x.name, session, translations)));
                this.tb_build_parts.Text = string.Join(",", build_records[this.dg_build_view_grid.Rows[e.RowIndex].Cells[0].Value.ToString()].parts.Select(x => translate.translate_string(x, session, translations)));
            }
        }

        private void build_view_Load(object sender, EventArgs e)
        {
        }

        private void cb_build_game_modes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_build_game_modes.SelectedIndex >= 0)
                game_mode_filter = this.cb_build_game_modes.Text;

            populate_build_record_table();
        }

        private void dg_build_view_grid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (this.dg_build_view_grid.SelectedCells.Count == 0)
                return;

            List<int> selected_rows = new List<int> { };
            List<int> selected_columns = new List<int> { };

            foreach (DataGridViewCell cell in this.dg_build_view_grid.SelectedCells)
            {
                if (!selected_rows.Contains(cell.RowIndex))
                    selected_rows.Add(cell.RowIndex);
                if (!selected_columns.Contains(cell.ColumnIndex))
                    selected_columns.Add(cell.ColumnIndex);
            }

            if (selected_rows.Contains(e.RowIndex))
                e.AdvancedBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            else
                e.AdvancedBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;

            if (selected_columns.Contains(e.ColumnIndex))
                e.AdvancedBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.Single;
            else
                e.AdvancedBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
        }

        private void cb_grouped_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_grouped.SelectedIndex >= 0)
                group_filter = this.cb_grouped.Text;

            populate_build_record_table();
        }

        private void cb_map_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_map.SelectedIndex >= 0)
                map_filter = this.cb_map.Text;

            populate_build_record_table();
        }

        private void cb_power_score_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_power_score.SelectedIndex >= 0)
                power_score_filter = this.cb_power_score.Text;

            populate_build_record_table();
        }

        private void cb_client_version_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cb_client_version.SelectedIndex >= 0)
                client_versions_filter = this.cb_client_version.Text;

            populate_build_record_table();
        }

        private void btn_reset_filters_Click(object sender, EventArgs e)
        {
            reset_filters();
            game_mode_filter = global_data.GAME_MODE_FILTER_DEFAULT;
            group_filter = global_data.GROUP_FILTER_DEFAULT;
            map_filter = global_data.MAP_FILTER_DEFAULT;
            power_score_filter = global_data.POWER_SCORE_FILTER_DEFAULT;
            client_versions_filter = global_data.CLIENT_VERSION_FILTER_DEFAULT;
            populate_build_record_table();
        }

        private void dg_build_view_grid_SelectionChanged(object sender, EventArgs e)
        {
            this.dg_build_view_grid.Invalidate();
        }
    }
}

